using UnityEngine;

/// <summary>
/// Height Map interface that can get and set the height at a specified x and y coordinate.
/// </summary>
abstract public class HeightMap : MonoBehaviour {
    /// <summary>
    /// Gets the height of the map at a given x, y coordinate.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <returns>Height at specified position as a float.</returns>
    abstract public float GetHeight(int x, int y);

    /// <summary>
    /// Sets the height value at a specified x, y coordinate.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <param name="height">Height to set at position x and y.</param>
    abstract public void SetHeight(int x, int y, float height);

    /// <summary>
    /// Checks if a coordinate is in the bounds of the heightmap.
    /// </summary>
    /// <param name="x">X position in the height map</param>
    /// <param name="y">Y position in the height map</param>
    /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
    abstract public bool IsInBounds(int x, int y);
}