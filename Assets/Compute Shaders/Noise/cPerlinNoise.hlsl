# ifndef __PERLIN_NOISE_HLSL__
# define __PERLIN_NOISE_HLSL__

#include "Assets/Compute Shaders/Noise/iNoise.hlsl"

/// <summary>
/// Using Perlin Noise function from adrian's soapbox "Understanding Perlin Noise" by Flafla2.
/// https://flafla2.github.io/2014/08/09/perlinnoise.html
/// From August 9th, 2014
/// 
/// github link: https://gist.github.com/Flafla2/f0260a861be0ebdeef76
/// 
/// A high level overview of Perlin Noise is that it tries to generate smooth, natural
/// looking curves. This allows for natural looking terrain, handwriting, etc...
/// 
/// The way Perlin Noise works is that it generates a grid of vectors at each 
/// coordinate in a n-dimensional space. The value of noise at each position in
/// the space is between -1 and 1 (converted to [0,1] for this example). This value
/// is determined by the direction of the vectors in which a coordinate is located.
/// If more of the neighboring vectors at the corners of the cell are pointing towards
/// a point, then the value is closer to 1. If the vectors are pointing away from
/// a point, then the values are closer to zero. This is calculated by using a gradient
/// function to linearly interpolate how close the value should be zero or one.
/// There is additionally a fading value that is used to smooth out the curves 
/// so they aren't block and rough.
/// 
/// They way this specific implementation works is by using a random hash function
/// to calculate a vector direction for each integer coordinate in 3d space. 
/// This hash function provides the direction of the vectors so the gradient vectors
/// do not have to be saved.
/// </summary>
class cPerlinNoise : iNoise {
    /// <summary>
    /// Doubled permutation to avoid overflow.
    /// </summary>
    StructuredBuffer<int> permutation;
    /// <summary>
    /// Size of the permutation for noise function.
    /// </summary>
    uint permutationSize;
    /// <summary>
    /// Bounds the perlin noise to a grid space and repeats that grid square.
    /// </summary>
    uint repeat;

    /// <summary>
    /// Gets the hash of a position. This is a random hash function for a position's gradient vector.
    /// </summary>
    /// <param name="pos">Position in the grid space (three component vector) as integer values.</param>
    /// <returns>The hash of the position for creating gradient vectors.</returns> 
    int GetHashOfPosition(int3 pos) {
        int hash = pos.x;
        hash = permutation[hash % permutationSize] + pos.y;
        hash = permutation[hash % permutationSize] + pos.z;
        hash = permutation[hash % permutationSize];
        return hash;
    }

    /// <summary>
    /// Computes the linear interpolation (Lerp) of a and b by some factor x
    /// </summary>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    /// <param name="x">Proportional value</param>
    /// <returns>Linear interpolation of a and b by x</returns>
    float Lerp(float a, float b, float x) {
        return a + x * (b - a);
    }

    /// <summary>
    /// Calculates the dot product of a randomly selected gradient vector and the 8 location vectors.
    /// </summary>
    /// <param name="hash">Hash value at a given position.</param>
    /// <param name="vec">Gradient vector with x, y, z components.</param>
    /// <returns>Computes the dot product of the gradient vector given the hash value.</returns>
    float Grad(int hash, float3 vec) {
        switch(hash & 0xF)
        {
            case 0x0: return  vec.x + vec.y;
            case 0x1: return -vec.x + vec.y;
            case 0x2: return  vec.x - vec.y;
            case 0x3: return -vec.x - vec.y;
            case 0x4: return  vec.x + vec.z;
            case 0x5: return -vec.x + vec.z;
            case 0x6: return  vec.x - vec.z;
            case 0x7: return -vec.x - vec.z;
            case 0x8: return  vec.y + vec.z;
            case 0x9: return -vec.y + vec.z;
            case 0xA: return  vec.y - vec.z;
            case 0xB: return -vec.y - vec.z;
            case 0xC: return  vec.y + vec.x;
            case 0xD: return -vec.y + vec.z;
            case 0xE: return  vec.y - vec.x;
            case 0xF: return -vec.y - vec.z;
            default: return 0; // never happens
        }
    }

    /// <summary>
    /// Helper function to get the linear interpolation 
    /// of two positions.
    /// </summary>
    /// <param name="pos">Position in the grid.</param>
    /// <param name="fraction">Fractional position in the grid</param>
    /// <param name="corner1">First corner of interpolation</param>
    /// <param name="corner2">Second corner of interpolation</param>
    /// <param name="fade">Amount to fade between corners</param>
    /// <returns>The interpolation between two gradients</returns>
    float GetLerpOfCorner(int3 pos, float3 fraction, int3 corner1, int3 corner2, float fade) {
        float corner1gradient = Grad (GetHashOfPosition(pos + corner1), fraction - corner1),
            corner2gradient = Grad (GetHashOfPosition(pos + corner2), fraction - corner2);
        return Lerp(corner1gradient, corner2gradient, fade);
    }

    /// <summary>
    /// Fade function as defined by Ken Perlin.  This eases coordinate values
    /// so that they will ease towards integral values.  This ends up smoothing
    /// the final output.
    /// </summary>
    /// <param name="t"></param>
    /// <returns>6t^5 - 15t^4 + 10t^3</returns>
    float Fade(float t) {                                            
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Calculates the Perlin Noise value at a given x, y, z position.
    /// </summary>
    /// <param name="vec">Vector position to computer Perlin Noise with x, y, z component</param>
    /// <returns>Perlin noise value at the given coordinate. Will always be in the range [0.0, 1.0]</returns>
    float GetNoise(float3 vec) {
        // If we have any repeat on, change the coordinates to their "local" repetitions
        if (repeat > 0) {
            vec = float3(vec.x % repeat, vec.y % repeat, vec.z % repeat);
        }

        // Calculate the "unit cube" that the point asked will be located in
        // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        int3 posInt = int3(floor(vec.x), floor(vec.y), floor(vec.z));
        float3 posFrac = float3(vec.x - floor(vec.x), vec.y - floor(vec.y), vec.z - floor(vec.z));

        // Compute faded values for smoothing
        float3 vecFade = float3(Fade(posFrac.x), Fade(posFrac.y), Fade(posFrac.z));

        float x1, x2, y1, y2;
        // The gradient function calculates the dot product between a pseudorandom
        // gradient vector and the vector from the input coordinate to the 8
        // surrounding points in its unit cube.
        // This is all then combined using a lerp together as a sort of
        // weighted average based on the faded (u,v,w)
        
        // Find the lerp of the gradients at corners 000 and 100
        x1 = GetLerpOfCorner(posInt, posFrac, int3(0,0,0), int3(1,0,0), vecFade.x);
        // Find the lerp of the gradients at corners 010 and 110
        x2 = GetLerpOfCorner(posInt, posFrac, int3(0,1,0), int3(1,1,0), vecFade.x);
        y1 = Lerp(x1, x2, vecFade.y);

        // Find the lerp of the gradients at corners 001 and 101
        x1 = GetLerpOfCorner(posInt, posFrac, int3(0,0,1), int3(1,0,1), vecFade.x);
        // Find the lerp of the gradients at corners 011 and 111
        x2 = GetLerpOfCorner(posInt, posFrac, int3(0,1,1), int3(1,1,1), vecFade.x);
        y2 = Lerp (x1, x2, vecFade.y);

        // For convenience we bind the result to 0 - 1 (theoretical min/max before is [-1, 1])
        return (Lerp (y1, y2, vecFade.z)+1)/2;
    }
};

/// <summary>
/// Makes a perlin noise generator with given settings.
/// </summary>
/// <param name="repeat">Bounds noise to a repeating square of size repeat by repeat. If this 
/// value is set to zero, there is no repeat.</param>
/// <param name="permutation">Unique array of permutationSize to hold permutation elements of values (0,permutationSize]</param>
/// <param name="permutationSize">Size of permutation array</param>
cPerlinNoise CreatePerlinNoise(StructuredBuffer<int> permutation, uint permutationSize, uint repeat) {
    cPerlinNoise noise;
    noise.permutation = permutation;
    noise.permutationSize = permutationSize;
    noise.repeat = repeat;
    return noise;
}

#endif //__PERLIN_NOISE_HLSL__
