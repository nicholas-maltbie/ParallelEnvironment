using System;
using System.Linq;
using Terrain.Map;
using UnityEngine;

namespace Terrain.Erosion {

    /// <summary>
    /// Performs Hydraulic Erosion in Parallel using a Compute Shader,
    /// as many droplets at a time as possible.
    /// <see cref="AbstractHydroErosion"/> for more details.
    /// </summary>
    public class GPUHydroErosion : IHydroErosion {
        /// <summary>
        /// Does erosion in Parallel using Spin Locks
        /// <see cref="AbstractHydroErosion.DoErosion"/> for more information
        /// <seealso cref="AbstractHydroErosion"/>
        /// </summary>
        public IChangeMap DoErosion(IHeightMap heightMap, Vector2Int start, Vector2Int end, int iterations,
            HydroErosionParams erosionParams, System.Random prng) {
            
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            int numThreads = Mathf.Max(iterations / 128, 1);

            int mapDimX = end.x - start.x;
            int mapDimY = end.y - start.y;

            // Compute slice of the height map
            float[] heightMapSlice = new float[mapDimX * mapDimY];
            ParallelEnumerable.Range(0, mapDimX * mapDimY).ForAll(
                i => {
                    int x = i % mapDimX;
                    int y = i / mapDimX;
                    heightMapSlice[x + y * mapDimX] = heightMap.GetHeight(x, y);
                }
            );

            ComputeShader erosionShander = erosionParams.erosionShader;
            int kernelIdx = erosionShander.FindKernel("Erode");

            // Set height map
            ComputeBuffer heightMapBuffer = new ComputeBuffer (mapDimX * mapDimY, sizeof(float));
            heightMapBuffer.SetData(heightMapSlice);
            erosionShander.SetBuffer(kernelIdx, "heightMap", heightMapBuffer);

            // Set locks for height map
            ComputeBuffer locksBuffer = new ComputeBuffer (mapDimX * mapDimY, sizeof(uint));
            locksBuffer.SetData(new uint[mapDimX * mapDimY]);
            erosionShander.SetBuffer(kernelIdx, "locks", locksBuffer);

            // Set map information
            erosionShander.SetInt("mapDimX", mapDimX);
            erosionShander.SetInt("mapDimY", mapDimY);

            // Set erosion brush
            float[] erodeBrush1D = new float[erosionParams.erodeBrush.GetLength(0) * erosionParams.erodeBrush.GetLength(1)];
            for (int x = 0; x < erosionParams.erodeBrush.GetLength(0); x++) {
                for (int y = 0; y < erosionParams.erodeBrush.GetLength(1); y++) {
                    erodeBrush1D[x + y * erosionParams.erodeBrush.GetLength(1)] = erosionParams.erodeBrush[x, y];
                }
            }
            ComputeBuffer erodeBrushBuffer = new ComputeBuffer (erodeBrush1D.Length, sizeof(float));
            erodeBrushBuffer.SetData(erosionParams.erodeBrush);
            erosionShander.SetBuffer(kernelIdx, "erodeBrush", erodeBrushBuffer);

            // Set erosion changes buffer
            ComputeBuffer erosionChangesBuffer = new ComputeBuffer (mapDimX * mapDimY, sizeof(float));
            float[] changes = new float[mapDimX * mapDimY];
            erosionChangesBuffer.SetData(changes);
            erosionShander.SetBuffer(kernelIdx, "erosionMap", erosionChangesBuffer);

            // Setup erosion parameters
            erosionShander.SetFloat("inertia", erosionParams.inertia);
            erosionShander.SetFloat("initialWater", erosionParams.initialWater);
            erosionShander.SetFloat("initialVelocity", erosionParams.initialVelocity);
            erosionShander.SetFloat("gravity", erosionParams.gravity);
            erosionShander.SetFloat("sedimentCapacityFactor", erosionParams.sedimentCapacityFactor);
            erosionShander.SetFloat("evaporationRate", erosionParams.evaporationRate);
            erosionShander.SetFloat("minSlope", erosionParams.minSlope);
            erosionShander.SetFloat("minCapacity", erosionParams.minCapacity);
            erosionShander.SetFloat("depositionRate", erosionParams.depositionRate);
            erosionShander.SetFloat("erodeRate", erosionParams.erodeRate);
            erosionShander.SetInt("maxDropletLifetime", erosionParams.maxDropletLifetime);
            erosionShander.SetInt("erodeRadius", erosionParams.erodeRadius);
            erosionShander.SetBool("includeVelocity", erosionParams.includeVelocity);

            // Run the command and get results
            erosionShander.Dispatch(kernelIdx, numThreads, 1, 1);
            erosionChangesBuffer.GetData(changes);

            // release buffers
            erodeBrushBuffer.Release();
            heightMapBuffer.Release();
            erosionChangesBuffer.Release();
            locksBuffer.Release();

            if (erosionParams.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Total Millis: " + deltaMillis + ", Millis Per Droplet: " + deltaMillis / iterations);
            }
            return new GPUChangeMap(mapDimX, mapDimY, changes, erosionParams.kernelShader);
        }
    }
}