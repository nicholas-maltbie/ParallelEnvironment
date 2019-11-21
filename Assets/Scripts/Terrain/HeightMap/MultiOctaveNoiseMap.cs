using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a height map using multiple octaves of noise generated with various settings.
/// 
/// Uses an OctaveNoise gen that uses a PerlinNoise for individual octaves.
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
    /// Higher values are smooth, lower values are noisier.
    /// </summary>
    [Range(2f, 1000f)]
    public float scaleFactor = 200;

    /// <summary>
    /// Creates a height map using multiple levels of perlin noise as specified in the parameters.
    /// </summary>
    /// <param name="mapSize">Size of the map along one edge (square map)</param>
    /// <returns>A height map of n combined octaves of perlin noise.
    /// This is a float array representing the height map where each location
    /// corresponds to index = y * mapSize + x. Each value in the map will be
    /// between 0.0 and 1.0</returns>
    public override float[] CreateHeightMap(int mapSize)
    {
        float[] heights = new float[mapSize * mapSize];
        PerlinNoise noiseGen = new PerlinNoise(this.repeat, this.seed);
        OctaveNoise octaveNoise = new OctaveNoise(noiseGen, this.octaves, this.persistence, this.frequencyGrowth);

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                heights[x + mapSize * y] = octaveNoise.GetNoise(
                    new Vector3(x / this.scaleFactor, y / this.scaleFactor, 1));
            }
        }
        
        return heights;
    }
}