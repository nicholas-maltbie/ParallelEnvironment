using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a height map using the PerlinNoise function
/// provided in the Mathf function.
/// </summary>
public class PerlinNoiseMap : AbstractHeightMapGenerator
{
    public override float[] CreateHeightMap(int mapSize)
    {
        // Create the square height map of mapSize * mapSize
        float[] heightMap = new float[mapSize * mapSize];

        // For each x and y coordinate
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                // Calculate the index in the height map
                int index = y * mapSize + x;
                /// assign the value based on the Mathf PerlinNoise function
                heightMap[index] = Mathf.PerlinNoise((float) x / mapSize,(float) y / mapSize);
            }
        }

        return heightMap;
    }
}
