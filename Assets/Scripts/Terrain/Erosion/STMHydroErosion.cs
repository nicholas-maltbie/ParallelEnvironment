using System;
using System.Linq;
using Shielded;
using Terrain.Map;
using UnityEngine;

namespace Terrain.Erosion {
    /// <summary>
    /// Performs Hydraulic Erosion in Parallel using State Transactional Memory,
    /// as many droplets at a time as possible.
    /// <see cref="AbstractHydroErosion"/> for more details.
    /// </summary>
    public class STMHydroErosion : IHydroErosion {
        /// <summary>
        /// Does erosion in Parallel Using State Transactional Memory
        /// <see cref="AbstractHydroErosion.DoErosion"/> for more information
        /// <seealso cref="AbstractHydroErosion"/>
        /// </summary>
        public void DoErosion(IHeightMap heightMap, Vector2Int start, Vector2Int end, int iterations,
            HydroErosionParams erosionParams, System.Random prng) {
            // Map for changes in current set of raindrops
            IChangeMap deltaMap = new ShieldedChangeMap(end.x - start.x, end.y - start.y);
            // Layered map for storing information about the original map and delta map together
            LayeredMap layers = new LayeredMap(deltaMap, heightMap);

            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            Action<int> raindrop = i => {
                Droplet droplet = Droplet.CreateRandomizedDroplet(prng, erosionParams, layers, 
                    start, end);
                Shield.InTransaction(() => {
                    Droplet.SimulateDroplet(droplet);
                });
            };

            // Iteration for each raindrop
            ParallelEnumerable.Range(0, iterations).ForAll(raindrop);

            if (erosionParams.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Total Millis: " + deltaMillis + ", Millis Per Droplet: " + deltaMillis / iterations);
            }

            // If bluring changes, do steps to blur map
            if (erosionParams.blurValue > 0) {
                // Calculate the blurred map by applying the blur brush kernel to the map
                IChangeMap blurredMap = deltaMap.ApplyKernel(erosionParams.blurBrush);
                // Multiply the original map and blurred map by ratios
                blurredMap.Multiply(erosionParams.blurValue);
                deltaMap.Multiply(1 - erosionParams.blurValue);

                // Apply changes to the original height map
                blurredMap.ApplyChangesToMap(heightMap);
                deltaMap.ApplyChangesToMap(heightMap);
            }
            // If not bluring changes, just ignore that complexity
            else {
                deltaMap.ApplyChangesToMap(heightMap);
            }
        }
    }
}