using UnityEngine;

namespace Erosion {
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
        /// Amount of water that the droplet is carying
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
        /// Parameters for controling droplet behaviour
        /// </summary>
        private ErosionParams erosionParams;
        /// <summary>
        /// prng for moving droplet when it is moving too slow
        /// </summary>
        private System.Random prng;
        /// <summary>
        /// Height map for movement
        /// </summary>
        private HeightMap map;

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
        public static Droplet CreateRandomizedDroplet(System.Random prng, ErosionParams erosionParams, HeightMap map,
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
        public Droplet(HeightMap map, Vector2 pos, ErosionParams erosionParams, System.Random prng) {
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
            bool outOfBounds = !this.map.IsInBounds(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
            bool outOfWater = this.water == 0;
            return tooOld || outOfBounds || outOfWater;
        }

        public void DoStep() {
            // Exit if already dead
            if (HasDied()) {
                return;
            }
            // increment steps by one
            this.steps += 1;

            // Compute gradient at current position
            Vector2 grad = ErosionUtils.CalculateGradient(this.map, this.pos);

            // Compute new direction as combination of old direction and gradient
            // Add some intertia for fun
            dir = dir * this.erosionParams.inertia - grad * (1 - this.erosionParams.inertia);

            // Select a random direction if dir is zero
            if (dir.x == 0 && dir.y == 0) {
                dir = new Vector2(this.prng.Next(), this.prng.Next());
            }

            // Normalize the vector dir so that it only moves on cell
            // at a time. This stops raindrops from skipping areas of the map.
            dir /= dir.magnitude;

            // Calculate the new position
            Vector2 posNew = this.pos + dir;

            // Calculate the change in height
            float heightOld = ErosionUtils.ApproximateHeight(this.map, this.pos);
            float heightNew = ErosionUtils.ApproximateHeight(this.map, posNew);
            float deltaH = heightNew - heightOld;

            // Calculate the carying capacity of the droplet
            float capacity = ErosionUtils.ComputeCapacity(deltaH, this.vel, this.water, this.erosionParams);

            // if droplet moved off the map or stopped moving, kill it
            if (this.water == 0) {
                // Don't deposit because this should have already been handled
                return;
            }
            else if (!map.IsInBounds(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y))) {
                // If the droplet had excess sediment, attempt to deposit it.
                if (this.sediment > 0) {
                    this.sediment -= ErosionUtils.DepositSediment(deltaH, this.sediment,
                        capacity, this.pos, this.map, this.erosionParams);
                }
                return;
            }

            // If the droplet is carying too much sediment, it will drop its sediment
            if (deltaH >= 0 || this.sediment > capacity) {
                this.sediment -= ErosionUtils.DepositSediment(deltaH, this.sediment, capacity,
                    this.pos, this.map, this.erosionParams);
            }
            // If the droplet is flowign downhill and has excess capacity, it will erode terrain
            else {
                float amountToErode = Mathf.Min((capacity - this.sediment) * this.erosionParams.erodeRate, -deltaH);
                this.sediment += ErosionUtils.Erode(this.map, this.pos, amountToErode, this.erosionParams.erodeRadius,
                    this.erosionParams.erodeBrush);
            }

            // Update velocity
            this.vel = Mathf.Sqrt(Mathf.Max(0, this.vel * this.vel + -deltaH * this.erosionParams.gravity));
            // Updater water
            this.water = this.water * (1 - this.erosionParams.evaporationRate);
            // Update position
            this.pos = posNew;
        }
    }
}