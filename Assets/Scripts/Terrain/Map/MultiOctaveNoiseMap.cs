using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Noise;
using System;

namespace Terrain.Map {
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
        public int permutationSize = 256;

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
        /// Type of execution for creating the height map.
        /// </summary>
        public GenerationType executionMethod;

        /// <summary>
        /// Compute shader for perlin noise functions.
        /// </summary>
        public ComputeShader perlinNoiseShader;

        /// <summary>
        /// Should performance statics be debugged to the screen.
        /// </summary>
        public bool debugPerformance;

        /// <summary>
        /// Generate the map using serial generation.
        /// </summary>
        /// <param name="mapSize">Size of the square map</param>
        /// <returns>Returns an array of floats between [0.0,1.0] in 
        /// an array of size mapSize * mapSize where the position
        /// x + y * mapSizeX is the value at position x,y.</returns>
        private float[] SerialGeneration(int mapSize) {
            float[] heights = new float[mapSize * mapSize];
            
            PerlinNoise noiseGen = new PerlinNoise(0, this.seed, this.permutationSize);
            OctaveNoise octaveNoise = new OctaveNoise(noiseGen, this.octaves, this.persistence, this.frequencyGrowth);

            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    heights[x + mapSize * y] = octaveNoise.GetNoise(
                        new Vector3(x / this.scaleFactor, y / this.scaleFactor, 1));
                }
            }

            return heights;
        }

        /// <summary>
        /// Generate the map using parallel threads.
        /// </summary>
        /// <param name="mapSize">Size of the square map</param>
        /// <returns>Returns an array of floats between [0.0,1.0] in 
        /// an array of size mapSize * mapSize where the position
        /// x + y * mapSizeX is the value at position x,y.</returns>
        private float[] ParallelGeneration(int mapSize) {
            float[] heights = new float[mapSize * mapSize];
            
            PerlinNoise noiseGen = new PerlinNoise(0, this.seed, this.permutationSize);
            OctaveNoise octaveNoise = new OctaveNoise(noiseGen, this.octaves, this.persistence, this.frequencyGrowth);

            ParallelEnumerable.Range(0, mapSize * mapSize).ForAll(
                i => {
                    int x = i % mapSize;
                    int y = i / mapSize;
                    heights[x + mapSize * y] = octaveNoise.GetNoise(
                        new Vector3(x / this.scaleFactor, y / this.scaleFactor, 1));
                }
            );
            return heights;
        }

        /// <summary>
        /// Generate the map using a GPU Compute shader.
        /// </summary>
        /// <param name="mapSize">Size of the square map</param>
        /// <returns>Returns an array of floats between [0.0,1.0] in 
        /// an array of size mapSize * mapSize where the position
        /// x + y * mapSizeX is the value at position x,y.</returns>
        private float[] GPUGeneration(int mapSize) {
            float[] heights = new float[mapSize * mapSize];
            
            int[] permutation = PerlinNoise.MakePermutation(256, this.seed);
            int kernelIdx = this.perlinNoiseShader.FindKernel("OctavePerlinMap");

            this.perlinNoiseShader.SetInt("mapSize", mapSize);
            this.perlinNoiseShader.SetInt("permutationSize", this.permutationSize);
            this.perlinNoiseShader.SetInt("repeat", 0);
            this.perlinNoiseShader.SetInt("octaves", this.octaves);
            this.perlinNoiseShader.SetFloat("persistence", this.persistence);
            this.perlinNoiseShader.SetFloat("frequencyGrowth", this.frequencyGrowth);
            this.perlinNoiseShader.SetFloat("scaleFactor", this.scaleFactor);
            
            ComputeBuffer permutationBuffer = new ComputeBuffer(256, sizeof(int));
            permutationBuffer.SetData(permutation);
            this.perlinNoiseShader.SetBuffer(kernelIdx, "permutation", permutationBuffer);
        
            ComputeBuffer resultsBuffer = new ComputeBuffer(mapSize * mapSize, sizeof(float));
            resultsBuffer.SetData(heights);
            this.perlinNoiseShader.SetBuffer(kernelIdx, "results", resultsBuffer);

            int numThreads = Mathf.Max(1, mapSize * mapSize / 32);
            this.perlinNoiseShader.Dispatch(kernelIdx, numThreads, 1, 1);
            resultsBuffer.GetData(heights);

            resultsBuffer.Release();
            permutationBuffer.Release();

            return heights;
        }

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
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            float[] heights;
            switch(this.executionMethod) {
                case GenerationType.GPU:
                    heights = GPUGeneration(mapSize);
                    break;
                case GenerationType.Parallel:
                    heights = ParallelGeneration(mapSize);
                    break;
                case GenerationType.Serial:
                default:
                    heights = SerialGeneration(mapSize);
                    break;
            }

            if (this.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Time to generate Perlin Noise Map: " + deltaMillis + " ms");
            }
            
            return heights;
        }
    }
}
