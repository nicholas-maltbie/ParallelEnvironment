using UnityEngine;
/// <summary>
/// Using Perlin Noise function from adrian's soapbox "Understanding Perlin Noise" by Flafla2.
/// https://flafla2.github.io/2014/08/09/perlinnoise.html
/// From August 9th, 2014
/// 
/// github link: https://gist.github.com/Flafla2/f0260a861be0ebdeef76
/// </summary>
public class PerlinNoise {

    /// <summary>
    /// Corners on a unit sphere, (0, 0, 0), (0,0,1), ..., (1, 1, 1)
    /// </summary>
    private static Vector3Int[] corners = new Vector3Int[8];

    /// <summary>
    /// Setup static variables of unit sphere
    /// </summary>
    static PerlinNoise() {
        // Perlin Noise hash function is used to get a unique value for every coordinate input. (8 in a unit sphere)
        for (int idx = 0; idx < 8; idx++) {
            // This will generate unit vectors for each corner of unit spehre
            // (0, 0, 0) -- 0
            // (0, 0, 1) -- 1
            // ...
            // (1, 1, 1) -- 7 
            corners[idx] = new Vector3Int((idx & 0b100) > 0 ? 1 : 0, (idx & 0b010) > 0 ? 1 : 0, (idx & 0b001) > 0 ? 1 : 0);
        }
    }

    /// <summary>
    /// Doubled permutation to avoid overflow.
    /// </summary>
    private int[] permutation;

    /// <summary>
    /// Bounds the perlin noise to a grid space and repeats that grid square.
    /// </summary>
    private int repeat = 0;

    /// <summary>
    /// Makes a perlin noise generator with given settings.
    /// </summary>
    /// <param name="repeat">Bounds noise to a repeating square of size repeat by repeat. If this 
    /// value is set to zero, there is no repeat.</param>
    /// /// <param name="seed">Seed value for random permutation. if set to zero a arbitrary seed will be used.</param>
    public PerlinNoise(int repeat, int seed) {
        int[] randomPermutation = new int[256];

        for (int i = 0; i < 256; i++) {
            randomPermutation[i] = i;
        }
        
        // Shuffle the permutation if a random seed is used.
        System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);
        int n = randomPermutation.Length;
        for (int i = n - 1; i > 1; i--) {
            int rnd = random.Next(i+1);
            int value = randomPermutation[rnd];
            randomPermutation[rnd] = randomPermutation[i];
            randomPermutation[i] = value;
        }

        // Double the permutation for overflow
        this.permutation = new int[512];
        for(int x=0;x<512;x++) {
            this.permutation[x] = randomPermutation[x % 256];
        }

        this.repeat = repeat;
    }

    /// <summary>
    /// Gets the hash of a position.
    /// </summary>
    /// <param name="pos">Position in the grid space (three component vector).</param>
    /// <returns>The hash of the position for creating gradient vectors.</returns>
    public int GetHashOfPosition(Vector3Int pos) {
        return 
            this.permutation[
                (this.permutation[
                    (this.permutation[pos.x % this.permutation.Length] +
                    pos.y) % this.permutation.Length] +
                pos.z) % this.permutation.Length];
    }

    /// <summary>
    /// Calculates the Perlin Noise value at a given x, y, z position.
    /// </summary>
    /// <param name="vec">Vector postiion to computer Perlin Noise with x, y, z component</param>
    /// <returns>Perlin noise value at the given coordinate. Will always be in the range [0.0, 1.0]</returns>
    public float Perlin(Vector3 vec) {
        // If we have any repeat on, change the coordinates to their "local" repetitions
        if (this.repeat > 0) {
            vec = new Vector3(vec.x % this.repeat, vec.y % this.repeat, vec.z % this.repeat);
        }

        // Calculate the "unit cube" that the point asked will be located in
        // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        Vector3Int posInt = new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
        Vector3 vecFraction = new Vector3(vec.x - (int)vec.x, vec.y - (int)vec.y, vec.z - (int)vec.z);

        // Compute faded values for smoothing
        Vector3 vecFade = new Vector3(Fade(vecFraction.x), Fade(vecFraction.y), Fade(vecFraction.z));

        float x1, x2, y1, y2;
        // The gradient function calculates the dot product between a pseudorandom
        // gradient vector and the vector from the input coordinate to the 8
        // surrounding points in its unit cube.
        // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
        
        // Find the lerp of the gradients at corners 000 and 100
        x1 = Lerp(Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b000]), vecFraction - PerlinNoise.corners[0b000]),
                  Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b100]), vecFraction - PerlinNoise.corners[0b100]),
                  vecFade.x);
        // Find the lerp of the gradients at corners 010 and 110
        x2 = Lerp(Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b010]), vecFraction - PerlinNoise.corners[0b010]),
                  Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b110]), vecFraction - PerlinNoise.corners[0b110]),
                  vecFade.x);
        y1 = Lerp(x1, x2, vecFade.y);

        // Find the lerp of the gradients at corners 001 and 101
        x1 = Lerp(Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b001]), vecFraction - PerlinNoise.corners[0b001]),
                  Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b101]), vecFraction - PerlinNoise.corners[0b101]),
                  vecFade.x);
        // Find the lerp of the gradients at corners 00111 and 111
        x2 = Lerp(Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b011]), vecFraction - PerlinNoise.corners[0b011]),
                  Grad (GetHashOfPosition(posInt + PerlinNoise.corners[0b111]), vecFraction - PerlinNoise.corners[0b111]),
                  vecFade.x);
        y2 = Lerp (x1, x2, vecFade.y);

        // For convenience we bind the result to 0 - 1 (theoretical min/max before is [-1, 1])
        return (Lerp (y1, y2, vecFade.z)+1)/2;
    }

    /// <summary>
    /// Computes multiple octave combination of perlin noise
    /// </summary>
    /// <param name="vec">Vector postiion to computer Perlin Noise with x, y, z component</param>
    /// <param name="octaves">Number of octaves to apply</param>
    /// <param name="persistence">Change in amplitude over each octave (decay)</param>
    /// <param name="frequencyGrowth">Growth in frequency over each octave (growth factor)</param>
    /// <returns>Returns the combination of multiple octaves of Perlin Noise.</returns>
    public float OctavePerlin(Vector3 vec, int octaves, float persistence, float frequencyGrowth) {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        // Used for normalizing result to 0.0 - 1.0
        float maxValue = 0;
        for(int i=0;i<octaves;i++) {
            total += Perlin(vec * frequency) * amplitude;
            
            maxValue += amplitude;
            
            amplitude *= persistence;
            frequency *= frequencyGrowth;
        }
        
        return total/maxValue;
    }


    /// <summary>
    /// Calculates the dot product of a randomly selected gradient vector and the 8 location vectors.
    /// </summary>
    /// <param name="hash">Hash value at a given position.</param>
    /// <param name="vec">Gradient vector with x, y, z components.</param>
    /// <returns>Computes the dot product of the gradient vector given the hash value.</returns>
    private float Grad(int hash, Vector3 vec) {
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
    /// Fade function as defined by Ken Perlin.  This eases coordinate values
    /// so that they will ease towards integral values.  This ends up smoothing
    /// the final output.
    /// </summary>
    /// <param name="t"></param>
    /// <returns>6t^5 - 15t^4 + 10t^3</returns>
    private float Fade(float t) {                                            
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Computes the linear interpolation (Lerp) of a and b by some factor x
    /// </summary>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    /// <param name="x">Proportional value</param>
    /// <returns>Linear interpolation of a and b by x</returns>
	public static float Lerp(float a, float b, float x) {
		return a + x * (b - a);
	}
}