namespace Terrain.Map {
    /// <summary>
    /// Change map, this is a performance map that starts
    /// with all values initialized to zero and should contain a 
    /// small delta to adjust a more persistent height map.
    /// </summary>
    public interface IChangeMap : IHeightMap {

        /// <summary>
        /// Adds all the changes stored in this map to another map (does a sum on the other map)
        /// </summary>
        /// <param name="targetMap"> Map to add changes to. </param>
        void ApplyChangesToMap(IHeightMap targetMap);

        /// <summary>
        /// Multiply all values in this map by a scalar. This changes this map.
        /// </summary>
        /// <param name="scalar">Scalar value to multiply and change all values in the map by.</param>
        void Multiply(float scalar);

        /// <summary>
        /// Gets a duplicate ChangeMap with the kernel applied to every element in the matrix.
        /// </summary>
        /// <param name="kernel">Kernel</param>
        /// <returns>Duplicate map with kernel applied to every cell</returns>
        IChangeMap ApplyKernel(float[,] kernel);
    }

    /// <summary>
    /// Static functions for any IChangeMap that are useful for any function.
    /// </summary>
    public static class IChangeMapExtensions {
        /// <summary>
        /// Run a kernel to a individual pixel. Returns the sum of the kernel applied to that pixel.
        /// Does NOT change the map.
        /// </summary>
        /// <param name="x">X position in grid (Center of kernel)</param>
        /// <param name="y">Y position in grid (Center of kernel)</param>
        /// <param name="kernel">Kernel to get summed value of</param>
        /// <returns>The sum of the weighted values around the center x and y. If a 
        /// value is not inside the grid, it is excluded for the weights and the other elements are weighted
        /// proportionally more.</returns>
        public static float Kernel(this IChangeMap map, int x, int y, float[,] kernel) {
            float totalWeights = 0;
            int width = kernel.GetLength(0);
            int height = kernel.GetLength(1);
            for (int kx = 0; kx < width; kx++) {
                for (int ky = 0; ky < height; ky++) {
                    if (map.IsInBounds(x + kx - width / 2, y + ky - height / 2)) {
                        totalWeights += kernel[kx, ky];
                    }
                }
            }

            float wSum = 0;
            for (int kx = 0; kx < width; kx++) {
                for (int ky = 0; ky < height; ky++) {
                    if (map.IsInBounds(x + kx - width / 2, y + ky - height / 2)) {
                        wSum += kernel[kx, ky] / totalWeights *
                                map.GetHeight(x - kx + width / 2, y + ky - height / 2);
                    }
                }
            }
            return wSum;
        }
    }
}