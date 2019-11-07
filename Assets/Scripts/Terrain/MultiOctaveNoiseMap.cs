using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a height map using multiple octaves of noise generated with various settings.
/// </summary>
public class MultiOctaveNoiseMap : AbstractHeightMapGenerator
{
    /// <summary>
    /// Number of different noise octaves to use
    /// </summary>
    [Range(1, 16)]
    public int octaves = 6;

    /// <summary>
    /// Initial frequency when computing noise for the first octave
    /// </summary>
    [Range(1, 64)]
    public float initialFrequency = 4;
    /// <summary>
    /// Initial amplitude when computing noise for the first octave
    /// </summary>
    [Range(1, 1024)]
    public float initialAmplitude = 128;

    /// <summary>
    /// Persistence in growth. Amplitude decreases by
    /// persistence^i after each ith octave.
    /// </summary>
    [Range(0, 1)]
    public float persistence = 0.5f;

    /// <summary>
    /// Growth in frequency after each iteration.
    /// frequency = frequencyGrowth^i after each ith octave
    /// </summary>
    [Range(1, 5)]
    public float frequencyGrowth = 2;

    public override float[] CreateHeightMap(int mapSize)
    {
        float[] heights = new float[mapSize * mapSize];
        return heights;
    }
}