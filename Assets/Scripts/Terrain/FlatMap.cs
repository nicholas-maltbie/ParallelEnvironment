using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a flat map at a set height.
/// </summary>
public class FlatMap : AbstractHeightMapGenerator
{
    [Range(0.0f, 1.0f)]
    public float height = 0;

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
