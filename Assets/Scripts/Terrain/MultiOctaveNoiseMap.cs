using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a height map using multiple octaves of noise generated with various settings.
/// 
/// Using Perlin Noise function from adrian's soapbox "Understanding Perlin Noise" by Flafla2.
/// https://flafla2.github.io/2014/08/09/perlinnoise.html
/// From August 9th, 2014
/// 
/// github link: https://gist.github.com/Flafla2/f0260a861be0ebdeef76
/// </summary>
public class MultiOctaveNoiseMap : AbstractHeightMapGenerator
{
    /// <summary>
    /// Number of different noise octaves to use
    /// </summary>
    [Range(1, 16)]
    public int octaves = 6;

    /// <summary>
    /// Repeat factor for perlin noise map. Use zero to have the map not repeat
    /// </summary>
    public int repeat = 0;

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
    [Range(2, 5)]
    public float frequencyGrowth = 2;

    /// <summary>
    /// Seed value for generating gradient for perlin noise. 
    /// </summary>
    public int seed;

    /// <summary>
    /// Scale factor for how noisy the map should be.
    /// Higher vlaues are smooth, lower values are noisier.
    /// </summary>
    [Range(2f, 1000f)]
    public float scaleFactor = 20;

    public override float[] CreateHeightMap(int mapSize)
    {
        float[] heights = new float[mapSize * mapSize];
        PerlinNoise noiseGen = new PerlinNoise(repeat, seed);

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                heights[x + mapSize * y] = noiseGen.OctavePerlin(
                    x / scaleFactor,
                    y / scaleFactor,
                    1,
                    octaves,
                    persistence,
                    frequencyGrowth);
            }
        }
        
        return heights;
    }
}