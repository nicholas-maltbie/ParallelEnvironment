/// <summary>
/// Using Perlin Noise function from adrian's soapbox "Understanding Perlin Noise" by Flafla2.
/// https://flafla2.github.io/2014/08/09/perlinnoise.html
/// From August 9th, 2014
/// 
/// github link: https://gist.github.com/Flafla2/f0260a861be0ebdeef76
/// </summary>
public class PerlinNoise {
    /// <summary>
    /// Doubled permutation to avoid overflow.
    /// </summary>
    private int[] p;

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
        int[] permutation = new int[256];

        for (int i = 0; i < 256; i++) {
            permutation[i] = i;
        }
        
        System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);
        int n = permutation.Length;
        for (int i = n - 1; i > 1; i--) {
            int rnd = random.Next(i+1);
            int value = permutation[rnd];
            permutation[rnd] = permutation[i];
            permutation[i] = value;
        }

        p = new int[512];
        for(int x=0;x<512;x++) {
            p[x] = permutation[x%256];
        }

        this.repeat = repeat;
    }

    /// <summary>
    /// Calculates the Perlin Noise value at a given x, y, z.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="z">Z coordinate</param>
    /// <returns>Perlin noise value at the given coordinate.</returns>
    public float Perlin(float x, float y, float z) {
        // If we have any repeat on, change the coordinates to their "local" repetitions
        if (repeat > 0) {
            x = x % repeat;
            y = y % repeat;
            z = z % repeat;
        }

        // Calculate the "unit cube" that the point asked will be located in
        // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        int xi = (int)x & 255;
        int yi = (int)y & 255;
        int zi = (int)z & 255;
        float xf = x - (int)x;
        float yf = y - (int)y;
        float zf = z - (int)z;

        // Compute faded values for smoothing
        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        // Perlin Noise hash function is used to get a unique value for every coordinate input.
        // This is the hash for each of the 8 points on the unit cube around this point
        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[    xi ]+    yi ]+    zi ];
        aba = p[p[p[    xi ]+Inc(yi)]+    zi ];
        aab = p[p[p[    xi ]+    yi ]+Inc(zi)];
        abb = p[p[p[    xi ]+Inc(yi)]+Inc(zi)];
        baa = p[p[p[Inc(xi)]+    yi ]+    zi ];
        bba = p[p[p[Inc(xi)]+Inc(yi)]+    zi ];
        bab = p[p[p[Inc(xi)]+    yi ]+Inc(zi)];
        bbb = p[p[p[Inc(xi)]+Inc(yi)]+Inc(zi)];

        float x1, x2, y1, y2;
        // The gradient function calculates the dot product between a pseudorandom
        // gradient vector and the vector from the input coordinate to the 8
        // surrounding points in its unit cube.
        // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
        x1 = Lerp(Grad (aaa, xf  , yf  , zf),           
                  Grad (baa, xf-1, yf  , zf),             
                  u);                                     
        x2 = Lerp(Grad (aba, xf  , yf-1, zf),           
                  Grad (bba, xf-1, yf-1, zf),             
                  u);
        y1 = Lerp(x1, x2, v);

        x1 = Lerp(Grad (aab, xf  , yf  , zf-1),
                  Grad (bab, xf-1, yf  , zf-1),
                  u);
        x2 = Lerp(Grad (abb, xf  , yf-1, zf-1),
                  Grad (bbb, xf-1, yf-1, zf-1),
                  u);
        y2 = Lerp (x1, x2, v);

        // For convenience we bind the result to 0 - 1 (theoretical min/max before is [-1, 1])
        return (Lerp (y1, y2, w)+1)/2;
    }

    /// <summary>
    /// Computes multiple octave combination of perlin noise
    /// </summary>
    /// <param name="x">X value to use in coordinate space</param>
    /// <param name="y">y value to use in coordinate space</param>
    /// <param name="z">z value to use in coordinate space</param>
    /// <param name="octaves">Number of octaves to apply</param>
    /// <param name="persistence">Change in amplitude over each octave (decay)</param>
    /// <param name="frequencyGrowth">Growth in frequency over each octave (growth factor)</param>
    /// <returns></returns>
    public float OctavePerlin(float x, float y, float z, int octaves, float persistence, float frequencyGrowth) {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        // Used for normalizing result to 0.0 - 1.0
        float maxValue = 0;
        for(int i=0;i<octaves;i++) {
            total += Perlin(x * frequency, y * frequency, z * frequency) * amplitude;
            
            maxValue += amplitude;
            
            amplitude *= persistence;
            frequency *= frequencyGrowth;
        }
        
        return total/maxValue;
    }


    /// <summary>
    /// Calculates the dot product of a randomly selected gradient vector and the 8 location vectors.
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private float Grad(int hash, float x, float y, float z) {
        switch(hash & 0xF)
        {
            case 0x0: return  x + y;
            case 0x1: return -x + y;
            case 0x2: return  x - y;
            case 0x3: return -x - y;
            case 0x4: return  x + z;
            case 0x5: return -x + z;
            case 0x6: return  x - z;
            case 0x7: return -x - z;
            case 0x8: return  y + z;
            case 0x9: return -y + z;
            case 0xA: return  y - z;
            case 0xB: return -y - z;
            case 0xC: return  y + x;
            case 0xD: return -y + z;
            case 0xE: return  y - x;
            case 0xF: return -y - z;
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
    ///  Increments a number but bounds the output to the range [0, repeat). and this wraps around.
    /// </summary>
    /// <param name="num">Input value to increment</param>
    /// <returns>num incremented by 1 bounded in the range [0, repeat)</returns>
    private int Inc(int num) {
        num++;
        if (repeat > 0) {
            num %= repeat;
        }
        return num;
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