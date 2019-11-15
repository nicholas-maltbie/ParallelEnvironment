using UnityEngine;
using System;

namespace Erosion {
    /// <summary>
    /// Simulated hydraulic erosion following 
    /// Hans Theobald Beyer's "Implementation of a method for Hydraulic Erosion"
    /// Bachelor's Thesis in Informatics at TECHNISCHE UNIVERSITÄT MÜNCHEN. 15.11.2015
    /// 
    /// https://www.firespark.de/resources/downloads/implementation%20of%20a%20methode%20for%20hydraulic%20erosion.pdf
    /// 
    /// Some of the documentation of parameters is directly from this document so if
    /// there is any uncertainty there is further explanation in this paper.
    /// 
    /// Simulates Hydraulic erosion by spawning raindrops around the map. As the raindrops
    /// move down the sides of the terrain they will erode sides of the mountian.
    /// The raindrop's movement at each individual step will be one cell in the (x,y) direction.
    /// If the raindrop is moving downhill quickly it will pick up sediment.
    /// As the raindrop picks up sediment, it will fill up and slowly deposit sediment if it
    /// becomes too full or slows down enough. 
    /// The raindrop will also slowly lose water and will die if it moves off the map
    /// or runs out of water completely. 
    /// </summary>
    public class HydroErosion : MonoBehaviour {
        /// <summary>
        /// Seed value for random number generator
        /// </summary>
        public int seed = 0;

        /// <summary>
        /// Inertia value for how much momentum is preserved.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float inertia = 0.05f;

        /// <summary>
        /// Starting water in each droplet.
        /// </summary>
        public float initialWater = 1;
        /// <summary>
        /// Starting velocity of each droplet.
        /// </summary>
        public float initialVelocity = 1;

        /// <summary>
        /// Force of gravity to pull droplets down gradient. Overall
        /// a higher gravity factor leads to faster erosion, but there are no differences in the
        /// appearance of the terrain.
        /// </summary>
        public float gravity = 9.81f;

        /// <summary>
        /// Should velocity be included in computing a droplets carry capacity.
        /// </summary>
        public bool includeVelocity = false;

        /// <summary>
        /// determines the amount of sediment a drop can carry as used
        /// in equation 5.4. A higher value results in more sediment being eroded on steeper
        /// ground and deposited in lower regions. Thus each drop has a higher impact on the 
        /// result. That leads to a rugged terrain with more ravines. 
        /// </summary>
        public float sedimentCapacityFactor = 4;

        /// <summary>
        /// determines how fast the drops evaporate. Again its value is 
        /// between 0 and 1. A faster evaporation leads to shorter paths of the drops in which they 
        /// influence the terrain.
        /// </summary>
        public float evaporationRate = 0.0123f;

        /// <summary>
        /// the minimum level of height difference that is taken for
        /// the calculation of the carry capacity of each single drop. Increasing the value ensures
        /// that the carry capacity does not fall below a certain line. Higher values lead to faster
        /// erosion but also stronger ravine forming. Terrains eroded with a low minimal slope
        /// need more drops but give smoother, more realistic looking terrains.
        /// </summary>
        public float minSlope = 0.01f;

        /// <summary>
        /// Minimum capacity of a raindrop. It will always be able to hold at least this much sediment. 
        /// </summary>
        public float minCapacity = 0.1f;

        /// <summary>
        /// Maximum time that a droplet can survive
        /// </summary>
        public int maxDropletLifetime = 256;

        /// <summary>
        /// limits the sediment that is dropped if the sediment carried
        /// by a drop exceeds the drops carry capacity. The value is
        /// between 0 and 1. Since the drop loses water over time through evaporation, it happens,
        /// that the capacity falls below the amount of currently carried sediment. For high values
        /// of deposition that leads to visible sediment deposition on the flow path
        /// </summary>
        [Range(0, 1)]
        public float depositionRate = 0.1f;
        
        /// <summary>
        /// determines how much of the free
        /// capacity of a drop is filled with sediment in case of erosion.
        /// The value is between 0 and 1. With a high erosion speed, a drop quickly fills its
        /// capacity and after that most likely only deposits sediment. With a low value, the drops
        /// pick up sediment for a longer path, which results in stronger ravine formation.
        /// </summary>
        [Range(0, 1)]
        public float erosionRate = 0.1f;

        /// <summary>
        /// determines the radius in which sediment is taken from the
        /// rock layer. The smaller radius is, the deeper and more distinct the ravines will be.
        /// Raising the erosion radius also increases the computational time needed for each drop
        /// drastically
        /// </summary>
        public int erodeRadius = 3;

        /// <summary>
        /// Amount of blur to use when applying erosion changes. Blurs change map before combining
        /// with regular map. Makes program significantly slower but does lead to smoother looking
        /// results. A blur is applied after a set of specified raindrops are created. The erosion
        /// map is blurred and the original erosion map and blurred map are combined in rations of
        /// original * (1 - blurValue) + blurred * (blurValue). Smoothing will look nicer when the
        /// program is run in larget batches.
        /// </summary>
        [Range(0, 1)]
        public float blurValue = 0f;
        /// <summary>
        /// Size of blur filter. Larger blur filter will fade the changes more. This specific
        /// implementation uses a gaussian blur filter where the standard deviation
        /// is the size of the filter radius.
        /// </summary>
        public int blurRadius = 2;

        /// <summary>
        /// Pseudo-random number generator for behaviour of raindrops
        /// </summary>
        private System.Random prng;

        /// <summary>
        /// Container for all erosion parameters
        /// </summary>
        private ErosionParams erosionParams;
        /// <summary>
        /// Have erosion parameters been setup yet
        /// </summary>
        private bool setupParams;

        /// <summary>
        /// Initializes the erosion parameters and prng
        /// </summary>
        private void Initialize() {
            // setup PRNG
            if (this.prng == null) {
                this.prng = this.seed == 0 ? new System.Random() : new System.Random(seed);
            }

            // Setup erosion parameters
            if (!setupParams) {
                this.setupParams = true;
                this.erosionParams = new ErosionParams(this.inertia, this.initialWater, 
                    this.initialVelocity, this.gravity, this.includeVelocity,
                    this.sedimentCapacityFactor, this.evaporationRate, this.minSlope,
                    this.minCapacity, this.maxDropletLifetime, this.depositionRate,
                    this.erosionRate, this.erodeRadius, this.blurValue, this.blurRadius);
            }
        }

        /// <summary>
        /// Erodes a hight map by generating a set of droplets then simulating their movement along the height map.
        /// </summary>
        /// <param name="map">Map to apply changes to.</param>
        /// <param name="start">Minimum location for spawning droplets (X,Y) position</param>
        /// <param name="end">Maximum location for spawning droplets (X,Y) position</param>
        /// <param name="iterations">Number of droplets to create</param>
        public void ErodeHeightMap(HeightMap heightMap, Vector2Int start, Vector2Int end, int iterations) {
            Initialize();

            // Map for changes in current set of raindrops
            ChangeMap deltaMap = new ChangeMap(end.x - start.x, end.y - start.y);
            // Layered map for storing information about the original map and delta map together
            LayeredMap layers = new LayeredMap(deltaMap, heightMap);
            
            // Iteration for each raindrop
            for (int iter = 0; iter < iterations; iter++) {
                Droplet droplet = Droplet.CreateRandomizedDroplet(this.prng, this.erosionParams, layers, 
                    start, end);
                Droplet.SimulateDroplet(droplet);
            }

            // If bluring changes, do steps to blur map
            if (this.blurValue > 0) {
                // Calculate the blurred map by applying the blur brush kernel to the map
                ChangeMap blurredMap = deltaMap.ApplyKernel(this.erosionParams.blurBrush);
                // Multiply the original map and blurred map by ratios
                blurredMap.Multiply(this.blurValue);
                deltaMap.Multiply(1 - this.blurValue);

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
