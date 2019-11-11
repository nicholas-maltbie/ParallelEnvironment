using System;
using System.Collections.Generic;

/// <summary>
/// A height map that is meant to hold changes. Starts will all avlues
/// initialized to zero and can be applied to other height maps.
/// </summary>
public class ChangeMap : HeightMap {
    /// <summary>
    /// Data stored in change map
    /// </summary>
    private float[] map;
    /// <summary>
    /// Dimensions of the map in the x and y axis
    /// </summary>
    private int dimX, dimY;

    /// <summary>
    /// Creates a change map with a given set of dimensions.
    /// </summary>
    /// <param name="sizeX">Size of the map along the X axis</param>
    /// <param name="sizeY">Size of teh map along the Y axis</param>
    public ChangeMap(int sizeX, int sizeY) {
        this.map = new float[sizeX * sizeY];
        this.dimX = sizeX;
        this.dimY = sizeY;
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
        x = Math.Min(Math.Max(0, x), this.dimX - 1);
        y = Math.Min(Math.Max(0, y), this.dimY - 1);
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
    public void ApplyChangesToMap(HeightMap targetMap) {
        for (int x = 0; x < this.dimX; x++) {
            for (int y = 0; y < this.dimY; y++) {
                targetMap.AddHeight(x, y, GetHeight(x, y));
            }
        }
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
    /// Run a kernel to a individual pixel. Returns the sum of the kernel applied to that pixel.
    /// Does NOT change the map.
    /// </summary>
    /// <param name="x">X position in grid (Center of kernel)</param>
    /// <param name="y">Y position in grid (Center of kernel)</param>
    /// <param name="kernel">Kernel to get summed value of</param>
    /// <returns>The sum of the weighted values around the center x and y. If a 
    /// value is not inside the grid, it is excluded for the weights and the other elements are wighted
    /// proportionally more.</returns>
    private float Kernel(int x, int y, float[,] kernel) {
        float totalWeights = 0;
        int width = kernel.GetLength(0);
        int height = kernel.GetLength(1);
        for (int kx = 0; kx < width; kx++) {
            for (int ky = 0; ky < height; ky++) {
                if (IsInBounds(x + kx - width / 2, y + ky - height / 2)) {
                    totalWeights += kernel[kx, ky];
                }
            }
        }

        float wSum = 0;
        for (int kx = 0; kx < width; kx++) {
            for (int ky = 0; ky < height; ky++) {
                if (IsInBounds(x + kx - width / 2, y + ky - height / 2)) {
                    wSum += kernel[kx, ky] / totalWeights *
                            GetHeight(x - kx + width / 2, y + ky - height / 2);
                }
            }
        }
        return wSum;
    }

    /// <summary>
    /// Gets a duplicate ChangeMap with the kernel applied to every element in the matrix.
    /// </summary>
    /// <param name="kernel">Kernel</param>
    /// <returns>Duplicate map with kernel applied to every cell</returns>
    public ChangeMap ApplyKernel(float[,] kernel) {
        ChangeMap applied = new ChangeMap(this.dimX, this.dimY);

        for (int x = 0; x < this.dimX; x++) {
            for (int y = 0; y < this.dimY; y++) {
                applied.SetHeight(x, y, Kernel(x, y, kernel));
            }
        }

        return applied;
    }
}