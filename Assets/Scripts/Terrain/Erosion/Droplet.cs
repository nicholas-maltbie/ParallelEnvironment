using UnityEngine;
using Terrain.Map;

namespace Terrain.Erosion {
    /// <summary>
    /// A droplet has the ability to move along a map and erode or deposit
    /// terrain depending on what has happened on the map.
    /// </summary>
    public class Droplet {
        /// <summary>
        /// Position of the droplet on the map
        /// </summary>
        private Vector2 pos;
        /// <summary>
        /// Amount of water that the droplet is carrying
        /// </summary>
        private float water;
        /// <summary>
        /// Current speed of the droplet
        /// </summary>
        private float speed;
        /// <summary>
        /// Velocity of the droplet
        /// </summary>
        private float vel;
        /// <summary>
        /// Number of steps that the droplet has taken
        /// </summary>
        private int steps;
        /// <summary>
        /// Current direction of movement of the droplet
        /// </summary>
        private Vector2 dir;
        /// <summary>
        /// Current amount of sediment that the droplet is carying
        /// </summary>
        private float sediment;
        /// <summary>
        /// Parameters for controlling droplet behaviour
        /// </summary>
        private HydroErosionParams erosionParams;
        /// <summary>
        /// prng for moving droplet when it is moving too slow
        /// </summary>
        private System.Random prng;
        /// <summary>
        /// Height map for movement
        /// </summary>
        private IHeightMap map;

        /// <summary>
        /// Creates a droplet with a randomized location.
        /// </summary>
        /// <param name="prng">Random number generator for droplet position and movement</param>
        /// <param name="erosionParams">Parameters for erosion</param>
        /// <param name="map">Height map for droplet</param>
        /// <param name="start">Starting range for droplet position</param>
        /// <param name="end">Ending range for droplet position</param>
        /// <returns>Creates a droplet at any position between [start + 1, end - 1] using
        /// a uniform distribution.</returns>
        public static Droplet CreateRandomizedDroplet(System.Random prng, HydroErosionParams erosionParams, IHeightMap map,
            Vector2Int start, Vector2Int end) {
            // Simulate each raindrop
            // Put the raindrop at a random position in the grid
            Vector2 pos = new Vector2(prng.Next(start.x + 1, end.x - 1), prng.Next(start.y + 1, end.y - 1));

            return new Droplet(map, pos, erosionParams, prng);
        }

        // Do a step of the droplet until the droplet dies
        public static void SimulateDroplet(Droplet droplet) {
            // Do steps until the droplet dies
            while(!droplet.HasDied()) {
                droplet.DoStep();
            }
        }
        
        /// <summary>
        /// Creates a droplet from given parameters and sets initial parameters
        /// from erosionParams.
        /// </summary>
        /// <param name="map">Height map that this droplet is moving on</param>
        /// <param name="pos">Current position of the droplet</param>
        /// <param name="erosionParams">Parameters controling the droplet behaviour</param>
        /// <param name="prng">Random numbers for moving droplet when its movement is
        /// too slow</param>
        public Droplet(IHeightMap map, Vector2 pos, HydroErosionParams erosionParams, System.Random prng) {
            this.map = map;
            this.pos = pos;
            this.erosionParams = erosionParams;
            this.water = erosionParams.initialWater;
            this.vel = erosionParams.initialVelocity;
            this.steps = 0;
            this.prng = prng;
            this.dir = Vector2.zero;
            this.sediment = 0;
        }

        /// <summary>
        /// Checks if this droplet HasDied yet. Will die when it has taken too many steps,
        /// all its water has evaporated, or when it moves out of bounds of the map.
        /// The droplet is in a superposition of being both alive and dead until this
        /// method has been invoked. Just like the cat I left in that box... Oh No, 
        /// I'm coming for you Whiskers!
        /// </summary>
        /// <returns>True if the droplet has died, false if it is still live.</returns>
        public bool HasDied() {
            bool tooOld = this.steps > this.erosionParams.maxDropletLifetime;
            bool outOfBounds = !this.map.IsInBounds(Mathf.FloorToInt(this.pos.x), Mathf.FloorToInt(this.pos.y));
            bool outOfWater = this.water == 0;
            return tooOld || outOfBounds || outOfWater;
        }

        /// <summary>
        /// Complete a single step in the droplets movement and erode/deposit on map if needed.
        /// </summary>
        public void DoStep() {
            // increment steps by one
            this.steps += 1;

            // Compute gradient at current position
            Vector2 grad = this.map.CalculateGradient(this.pos);

            // Compute new direction as combination of old direction and gradient
            // Add some intertia for fun
            this.dir = this.dir * this.erosionParams.inertia - grad * (1 - this.erosionParams.inertia);

            // Select a random direction if dir is zero
            if (this.dir.x == 0 && this.dir.y == 0) {
                this.dir = new Vector2(this.prng.Next(), this.prng.Next());
            }

            // Normalize the vector dir so that it only moves on cell
            // at a time. This stops raindrops from skipping areas of the map.
            this.dir /= this.dir.magnitude;

            // Calculate the new position
            Vector2 posNew = this.pos + this.dir;

            // Calculate the change in height
            float heightOld = this.map.ApproximateHeight(this.pos);
            float heightNew = this.map.ApproximateHeight(posNew);
            float deltaH = heightNew - heightOld;

            // Calculate the carying capacity of the droplet
            float capacity = ComputeCapacity(deltaH);

            // if droplet moved off the map or stopped moving, kill it
            if (this.water == 0 || !this.map.IsInBounds(Mathf.FloorToInt(posNew.x), Mathf.FloorToInt(posNew.y))) {
                this.sediment -= this.map.DepositSediment(deltaH, this.sediment, capacity,
                    this.pos, this.erosionParams);
                this.pos = posNew;
                return;
            }
            
            // If the droplet is carying too much sediment, it will drop its sediment
            if (deltaH >= 0 || this.sediment > capacity) {
                this.sediment -= this.map.DepositSediment(deltaH, this.sediment, capacity,
                    this.pos, this.erosionParams);
            }
            // If the droplet is flowign downhill and has excess capacity, it will erode terrain
            else {
                float amountToErode = Mathf.Min((capacity - this.sediment) * this.erosionParams.erodeRate, -deltaH);
                this.sediment += this.map.Erode(this.pos, amountToErode, this.erosionParams.erodeRadius,
                    this.erosionParams.erodeBrush);
            }

            // Update velocity
            this.vel = Mathf.Sqrt(Mathf.Max(0, this.vel * this.vel + -deltaH * this.erosionParams.gravity));
            // Updater water
            this.water = this.water * (1 - this.erosionParams.evaporationRate);
            // Update position
            this.pos = posNew;
        }

        /// <summary>
        /// Compute the capacity of a droplet using factors like include velocity, min slope, and capacity
        /// factor. This says how much a droplet can carry.
        /// </summary>
        /// <param name="deltaH">Change in height from previous movement</param>
        /// <param name="velocity">Current velocity of droplet</param>
        /// <param name="waterFactor">Amount of water in droplet</param>
        /// <param name="parameters">Erosion parameters for controlling how erosion works</param>
        /// <returns>The computed capacity of the droplet or Minimum capacity fi it is less than
        /// than the computed value.</returns>
        public float ComputeCapacity(float deltaH) {
            float slopeFactor = Mathf.Max(Mathf.Abs(deltaH), this.erosionParams.minSlope);
            float velFactor = Mathf.Max(1, this.erosionParams.includeVelocity ? this.vel : 1);
            float capacity = slopeFactor * velFactor * this.water * this.erosionParams.sedimentCapacityFactor;
            return Mathf.Max(Mathf.Min(capacity, Mathf.Abs(deltaH)), this.erosionParams.minCapacity);
        }
    }
}