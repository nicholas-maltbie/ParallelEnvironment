using UnityEngine;
using Terrain.Map;
using System;

namespace Terrain.Erosion {
    /// <summary>
    /// Performs Hydraulic Erosion in Serial, one droplet at a time
    /// <see cref="AbstractHydroErosion"/> for more details.
    /// </summary>
    public class SerialHydroErosion : IHydroErosion {

        /// <summary>
        /// Does erosion in Serial
        /// <see cref="AbstractHydroErosion.DoErosion"/> for more information
        /// <seealso cref="AbstractHydroErosion"/>
        /// </summary>
        public IChangeMap DoErosion(IHeightMap heightMap, Vector2Int start, Vector2Int end, int iterations,
            HydroErosionParams erosionParams, System.Random prng)
        {
            // Map for changes in current set of raindrops
            IChangeMap deltaMap = new ChangeMap(end.x - start.x, end.y - start.y);
            // Layered map for storing information about the original map and delta map together
            LayeredMap layers = new LayeredMap(deltaMap, heightMap);

            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            // Iteration for each raindrop
            for (int iter = 0; iter < iterations; iter++) {
                Droplet droplet = Droplet.CreateRandomizedDroplet(prng, erosionParams, layers, 
                    start, end);
                Droplet.SimulateDroplet(droplet);
            }

            if (erosionParams.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Total Millis: " + deltaMillis + ", Millis Per Droplet: " + deltaMillis / iterations);
            }
            return deltaMap;
        }
    }
}

