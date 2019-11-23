#ifndef __I_NOISE_HLSL__
#define __I_NOISE_HLSL__

/// <summary>
/// Noise function to generate noise values for a three dimensional space.
/// </summary>
interface iNoise {
    /// <summary>
    /// Will return a noise value between [0.0, 1.0] for a given (X,Y,Z) coordinate.
    /// </summary>
    /// <param name="position">Position in (X,Y,Z) space</param>
    /// <returns>The noise value at that coordinate, a float between [0.0, 1.0]</returns>
    float GetNoise(float3 position);
};

/// <summary>
/// Computes multiple octave combination of octaves of a noise function
/// </summary>
/// <param name="noise">Noise function to use for combining octaves</param>
/// <param name="octaves">Number of octaves to apply. Can be any number greater than one.</param>
/// <param name="persistence">Change in amplitude over each octave (decay). Can be any value between [0.0, 1.0]</param>
/// <param name="frequencyGrowth">Growth in frequency over each octave (growth factor)</param>
/// <param name="octaves">Number of octaves to apply. Can be any number greater than one.</param>
/// <param name="persistence">Change in amplitude over each octave (decay). Can be any value between [0.0, 1.0]</param>
/// <param name="frequencyGrowth">Growth in frequency over each octave (growth factor)</param>
/// <returns>Returns the combination of multiple octaves of Perlin Noise.</returns>
float GetOctaveNoise(iNoise noise, float3 position, int octaves, float persistence, float frequencyGrowth) {
    float total = 0;
    float frequency = 1;
    float amplitude = 1;
    // Used for normalizing result to 0.0 - 1.0
    float maxValue = 0;
    for(int i=0;i<octaves;i++) {
        total += noise.GetNoise(position * frequency) * amplitude;
        
        maxValue += amplitude;
        
        amplitude *= persistence;
        frequency *= frequencyGrowth;
    }
    
    return total/maxValue;
}

#endif //__I_NOISE_HLSL__
