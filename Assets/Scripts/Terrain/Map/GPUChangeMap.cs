
using UnityEngine;
using System.Linq;

namespace Terrain.Map {
    public class GPUChangeMap : IChangeMap {
        /// <summary>
        /// Data stored in change map
        /// </summary>
        private float[] map;
        /// <summary>
        /// Dimensions of the map in the x and y axis
        /// </summary>
        private int dimX, dimY;
        /// <summary>
        /// Kernel shader to apply a kernel to a this change map
        /// </summary>
        private ComputeShader kernelShader;

        /// <summary>
        /// Creates a change map with a given set of dimensions.
        /// </summary>
        /// <param name="dimX">Size of the map along the X axis</param>
        /// <param name="dimY">Size of teh map along the Y axis</param>
        /// <param name="map">Map of information from GPU operations</param>
        /// <param name="kernelShader">Kernel shader to apply a kernel to a this change map</param>
        public GPUChangeMap(int dimX, int dimY, float[] map, ComputeShader kernelShader) {
            this.map = map;
            this.dimX = dimX;
            this.dimY = dimY;
            this.kernelShader = kernelShader;
        }
        
        /// <summary>
        /// Gets the height of the map at a given x, y coordinate.
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <returns>Height at specified position as a float.</returns>
        private int GetIndex(int x, int y) {
            return x + y * this.dimX;
        }

        /// <summary>
        /// Adds to a map at the given coordinate by value of change
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <param name="change">Height to add at position x and y.</param>
        public void AddHeight(int x, int y, float change)
        {
            this.map[GetIndex(x, y)] += change;
        }

        /// <summary>
        /// Gets the height at a grid cell.If x and y are beyond the bounds of 
        /// the height map, the values are bounded to the closest edge.
        /// </summary>
        /// <param name="x">X coordinate in map</param>
        /// <param name="y">Y coordinate in map</param>
        /// <returns>Height saved at that specific x, y value. If x or y are < 0, the value less
        /// than zero is set to zero. If x or y are > mapSize - 1, then the value greater thn
        /// mapSize - 1 is set to mapSize - 1.</returns>
        public float GetHeight(int x, int y)
        {
            x = Mathf.Min(Mathf.Max(0, x), this.dimX - 1);
            y = Mathf.Min(Mathf.Max(0, y), this.dimY - 1);
            return this.map[GetIndex(x, y)];
        }

        /// <summary>
        /// Checks if a coordinate is in the bounds of the heightmap.
        /// </summary>
        /// <param name="x">X position in the height map</param>
        /// <param name="y">Y position in the height map</param>
        /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.dimX && y >= 0 && y < this.dimY;
        }

        /// <summary>
        /// Sets the height value at a specified x, y coordinate.
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <param name="height">Height to set at position x and y.</param>
        public void SetHeight(int x, int y, float height)
        {
            this.map[GetIndex(x, y)] = height;
        }

        /// <summary>
        /// Adds all the changes stored in this map to another map (does a sum on the other map)
        /// </summary>
        /// <param name="targetMap"> Map to add changes to. </param>
        public void ApplyChangesToMap(IHeightMap targetMap) {
            ParallelEnumerable.Range(0, this.dimX * this.dimY).ForAll( 
                i => {
                    int x = i % this.dimX;
                    int y = i / this.dimX;
                    float change = float.IsNaN(GetHeight(x, y)) ? 0 : GetHeight(x, y);
                    targetMap.AddHeight(x, y, change);
                }
            );
        }

        /// <summary>
        /// Multiply all values in this map by a scalar. This changes this map.
        /// </summary>
        /// <param name="scalar">Scalar value to multiply and change all values in the map by.</param>
        public void Multiply(float scalar) {
            for (int x = 0; x < this.dimX; x++) {
                for (int y = 0; y < this.dimY; y++) {
                    this.SetHeight(x, y, this.GetHeight(x, y) * scalar);
                }
            }   
        }

        /// <summary>
        /// Gets a duplicate ChangeMap with the kernel applied to every element in the matrix.
        /// This will utilize the GPU to make the changes.
        /// </summary>
        /// <param name="kernel">Kernel</param>
        /// <returns>Duplicate map with kernel applied to every cell</returns>
        public IChangeMap ApplyKernel(float[,] kernel) {
            int kernelSize = kernel.GetLength(0);
            float[] appliedMap = new float[this.dimX * this.dimY];
            float[] kernel1D = new float[kernelSize * kernelSize];

            int kernelIdx = this.kernelShader.FindKernel("Brush");
            for(int x = 0; x < kernelSize; x++) {
                for (int y = 0; y < kernelSize; y++) {
                    kernel1D[x + y * kernelSize] = kernel[x,y];
                }
            }
            ComputeBuffer kernelBuffer = new ComputeBuffer (kernelSize * kernelSize, sizeof(float));
            kernelBuffer.SetData(kernel1D);
            this.kernelShader.SetBuffer(kernelIdx, "kernel", kernelBuffer);
            
            // Set input buffer
            ComputeBuffer inputBuffer = new ComputeBuffer (this.dimX * this.dimY, sizeof(float));
            inputBuffer.SetData(this.map);
            this.kernelShader.SetBuffer(kernelIdx, "input", inputBuffer);
            // Set output buffer
            ComputeBuffer outputBuffer = new ComputeBuffer (this.dimX * this.dimY, sizeof(float));
            outputBuffer.SetData(appliedMap);
            this.kernelShader.SetBuffer(kernelIdx, "output", outputBuffer);

            // Setup blur parameters
            this.kernelShader.SetFloat("dimX", this.dimX);
            this.kernelShader.SetFloat("dimY", this.dimY);
            this.kernelShader.SetInt("kernelRadius", kernelSize / 2);

            // Run the command
            this.kernelShader.Dispatch(kernelIdx,
                Mathf.CeilToInt(this.dimX * this.dimY / 128.0f),
                1,
                1);

            // Get the results
            outputBuffer.GetData(appliedMap);

            // Recycle memory
            kernelBuffer.Release();
            inputBuffer.Release();
            outputBuffer.Release();

            GPUChangeMap applied = new GPUChangeMap(this.dimX, this.dimY, appliedMap, this.kernelShader);

            return applied;
        }

    }

}
