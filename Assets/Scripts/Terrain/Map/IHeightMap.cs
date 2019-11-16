using UnityEngine;

namespace Terrain.Map {
    /// <summary>
    /// Height Map interface that can get and set the height at a specified x and y coordinate.
    /// </summary>
    public interface IHeightMap {
        /// <summary>
        /// Gets the height of the map at a given x, y coordinate.
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <returns>Height at specified position as a float.</returns>
        float GetHeight(int x, int y);

        /// <summary>
        /// Sets the height value at a specified x, y coordinate.
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <param name="height">Height to set at position x and y.</param>
        void SetHeight(int x, int y, float height);

        /// <summary>
        /// Adds to a map at the given coordinate by value of change
        /// </summary>
        /// <param name="x">X position in grid</param>
        /// <param name="y">Y position in grid</param>
        /// <param name="change">Height to add at position x and y.</param>
        void AddHeight(int x, int y, float change);

        /// <summary>
        /// Checks if a coordinate is in the bounds of the heightmap.
        /// </summary>
        /// <param name="x">X position in the height map</param>
        /// <param name="y">Y position in the height map</param>
        /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
        bool IsInBounds(int x, int y);
    }
}