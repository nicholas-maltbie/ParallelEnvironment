using System;
using System.Linq;
using Terrain.Map;
using UnityEngine;

namespace Terrain.Erosion {

    /// <summary>
    /// Performs Hydraulic Erosion in Parallel using a Compute Shader,
    /// as many droplets at a time as possible.
    /// <see cref="AbstractHydroErosion"/> for more details.
    /// </summary>
    public class GPUHydroErosion : IHydroErosion {
        /// <summary>
        /// Does erosion in Parallel using Spin Locks
        /// <see cref="AbstractHydroErosion.DoErosion"/> for more information
        /// <seealso cref="AbstractHydroErosion"/>
        /// </summary>
        public IChangeMap DoErosion(IHeightMap heightMap, Vector2Int start, Vector2Int end, int iterations,
            HydroErosionParams erosionParams, System.Random prng) {
            
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            int numThreads = iterations / 8;

            int mapDimX = end.x - start.x;
            int mapDimY = end.y - start.y;

            // Compute slice of the height map
            float[] heightMapSlice = new float[mapDimX * mapDimY];
            ParallelEnumerable.Range(0, mapDimX * mapDimY).ForAll(
                i => {
                    int x = i % mapDimY;
                    int y = i % mapDimX;
                    heightMapSlice[x + y * mapDimX] = heightMap.GetHeight(x + start.x, y + start.y);
                }
            );

            ComputeShader erosionShander = erosionParams.computeShader;
            int kernelIdx = 0;

            // Setup random positions for each droplet
            int[] randomIndices = new int[iterations];
            for (int i = 0; i < iterations; i++) {
                int randomX = prng.Next (start.x, end.x);
                int randomY = prng.Next (start.y, end.y);
                randomIndices[i] = randomY * mapDimX + randomX;
            }

            // Send random indices to compute shader
            ComputeBuffer randomIndexBuffer = new ComputeBuffer (randomIndices.Length, sizeof (int));
            randomIndexBuffer.SetData (randomIndices);
            erosionShander.SetBuffer (kernelIdx, "randomIndices", randomIndexBuffer);

            // Set height map
            ComputeBuffer heightMapBuffer = new ComputeBuffer (mapDimX * mapDimY, sizeof(float));
            heightMapBuffer.SetData(heightMapSlice);
            erosionShander.SetBuffer(kernelIdx, "heightMap", heightMapBuffer);

            // Set map information
            erosionShander.SetInt("mapDimX", mapDimX);
            erosionShander.SetInt("mapDimY", mapDimY);

            // Set erosion brush
            ComputeBuffer erodeBrushBuffer = new ComputeBuffer (erosionParams.erodeBrush.Length, sizeof(float));
            erodeBrushBuffer.SetData(erosionParams.erodeBrush);
            erosionShander.SetBuffer(kernelIdx, "erodeBrush", erodeBrushBuffer);

            // Set erosion changes buffer
            ComputeBuffer erosionChangesBuffer = new ComputeBuffer (mapDimX * mapDimY, sizeof(float));
            float[] changes = new float[mapDimX * mapDimY];
            erosionChangesBuffer.SetData(changes);
            erosionShander.SetBuffer(kernelIdx, "erosionMap", erosionChangesBuffer);

            // Setup erosion parameters
            erosionShander.SetFloat("inertia", erosionParams.inertia);
            erosionShander.SetFloat("initialWater", erosionParams.initialWater);
            erosionShander.SetFloat("initialVelocity", erosionParams.initialVelocity);
            erosionShander.SetFloat("gravity", erosionParams.gravity);
            erosionShander.SetFloat("sedimentCapacityFactor", erosionParams.sedimentCapacityFactor);
            erosionShander.SetFloat("evaporationRate", erosionParams.evaporationRate);
            erosionShander.SetFloat("minSlope", erosionParams.minSlope);
            erosionShander.SetFloat("minCapacity", erosionParams.minCapacity);
            erosionShander.SetFloat("depositionRate", erosionParams.depositionRate);
            erosionShander.SetFloat("erodeRate", erosionParams.erodeRate);
            erosionShander.SetInt("maxDropletLifetime", erosionParams.maxDropletLifetime);
            erosionShander.SetInt("erodeRadius", erosionParams.erodeRadius);
            erosionShander.SetBool("includeVelocity", erosionParams.includeVelocity);

            // Run the command and get results
            erosionShander.Dispatch(erosionShander.FindKernel("CSMain"), numThreads, 1, 1);
            erosionChangesBuffer.GetData(changes);

            for (int i = 0; i < changes.Length; i++) {
                if (changes[i] > 0) {
                    Debug.Log((i % mapDimY) + ", " + (i / mapDimY) + ", " + changes[i]);
                }
            }

            // release buffers
            erodeBrushBuffer.Release();
            heightMapBuffer.Release();
            erosionChangesBuffer.Release();
            randomIndexBuffer.Release();

            if (erosionParams.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Total Millis: " + deltaMillis + ", Millis Per Droplet: " + deltaMillis / iterations);
            }
            return new GPUChangeMap(mapDimX, mapDimY, changes);
        }
    }
}