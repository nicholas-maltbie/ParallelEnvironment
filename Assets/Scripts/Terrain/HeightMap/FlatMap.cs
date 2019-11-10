using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a flat map at a set height.
/// </summary>
public class FlatMap : AbstractHeightMapGenerator
{
    /// <summary>
    /// Fixed height of every element in the map
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float height = 0;

    /// <summary>
    /// Creates a height map with a given map size at a fixed height.
    /// </summary>
    /// <param name="mapSize">Size of the map (squre)</param>
    /// <returns>All values in the float array are the same.null 
    /// The map is a float array represetnting the height map where each location
    /// corresponds to index = y * mapSize + x. Each value in the map will be
    /// between 0.0 and 1.0</returns>
    public override float[] CreateHeightMap(int mapSize)
    {
        // Create the square height map of mapSize * mapSize
        float[] heightMap = new float[mapSize * mapSize];

        // For each x and y coordinate
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                // Calculate the index in the height map
                int index = y * mapSize + x;
                /// assign the value based on the pre-set height
                heightMap[index] = height;
            }
        }

        return heightMap;
    }
}
