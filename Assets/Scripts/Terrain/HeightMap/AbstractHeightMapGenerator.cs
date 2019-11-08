using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class with the ability to create a height map upon request. 
/// </summary>
abstract public class AbstractHeightMapGenerator : MonoBehaviour
{
    /// <summary>
    /// Creates a height map of a square size based on settings. Values in
    /// the height map are in a range from 0.0f to 1.0f.
    /// </summary>
    /// <param name="mapSize">Size of length of square height map.</param>
    /// <returns>Returns a 1D array that represents the 2D height map
    /// of size mapSize * mapSize. Each index of the array represents
    /// the height at location y * mapSize + x.</returns>
    abstract public float[] CreateHeightMap(int mapSize);
}
