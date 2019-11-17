using UnityEngine;

namespace Terrain.Erosion {
    /// <summary>
    /// Set of parameters for eroding the height map
    /// </summary>
    public readonly struct HydroErosionParams {
        /// <summary>
        /// Creates a set of ErosionParameters
        /// </summary>
        /// <param name="inertia">Inertia of droplet movement.</param>
        /// <param name="initialWater">Initial water in each droplet</param>
        /// <param name="initialVelocity">Initial velocity of each droplet</param>
        /// <param name="gravity">Gravity pulling on each droplet</param>
        /// <param name="includeVelocity">Should velocity be included in droplet behaviour</param>
        /// <param name="sedimentCapacityFactor">Sediment capacity factor for holding sediment</param>
        /// <param name="evaporationRate">Rate at which the terrain is eroded</param>
        /// <param name="minSlope">Minmum slop for moving down hills</param>
        /// <param name="minCapacity">Minimum capacity of a droplet for sediment</param>
        /// <param name="maxDropletLifetime">Max number of steps that a droplet can survive</param>
        /// <param name="depositionRate">Rate at which droplets deposit sediment</param>
        /// <param name="erodeRate">Rate at which droplets erode sediment</param>
        /// <param name="erodeRadius">Erosion radius of a droplet</param>
        /// <param name="blurValue">Amount of bluring for terrain after changes</param>
        /// <param name="blurRadius">Radius of blurring terrain after changes</param>
        /// <param name="erodeBrush"></param>
        /// <param name="blurBrush"></param>
        public HydroErosionParams(float inertia, float initialWater, float initialVelocity, 
            float gravity, bool includeVelocity, float sedimentCapacityFactor, 
            float evaporationRate, float minSlope, float minCapacity, int maxDropletLifetime, float depositionRate,
            float erodeRate, int erodeRadius, float blurValue, int blurRadius) {
            this.inertia = inertia;
            this.initialWater = initialWater;
            this.initialVelocity = initialVelocity;
            this.gravity = gravity;
            this.includeVelocity = includeVelocity;
            this.sedimentCapacityFactor = sedimentCapacityFactor;
            this.evaporationRate = evaporationRate;
            this.minSlope = minSlope;
            this.minCapacity = minCapacity;
            this.maxDropletLifetime = maxDropletLifetime;
            this.depositionRate = depositionRate;
            this.erodeRate = erodeRate;
            this.erodeRadius = erodeRadius;
            this.blurValue = blurValue;
            this.blurRadius = blurRadius;
            this.erodeBrush = InitGaussianBrush(erodeRadius, erodeRadius / 3.0f);
            this.blurBrush = InitGaussianBrush(blurRadius, blurRadius / 3.0f);
        }


        /// <summary>
        /// Calculates a gaussian brush given a radius and standard deviation.
        /// </summary>
        /// <param name="radius">Radius of brush</param>
        /// <param name="sd">Standard deviation of gaussian distribution</param>
        /// <returns>A brush with a gaussian kernel centered at radius, radius of size 
        /// radius * 2 + 1, radius * 2 + 1</returns>
        public static float[,] InitGaussianBrush(int radius, float sd) {
            float[,] brush = new float[radius * 2 + 1, radius * 2 + 1];
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    brush[x + radius,y + radius] =
                        Mathf.Exp(- (x * x + y * y) / (2.0f * sd * sd)) / Mathf.Sqrt(2.0f * Mathf.PI * sd * sd);
                }
            }
            return brush;
        }

        /// <summary>
        /// Inertia value for how much momentum is preserved.
        /// </summary>
        public readonly float inertia;

        /// <summary>
        /// Starting water in each droplet.
        /// </summary>
        public readonly float initialWater;
        /// <summary>
        /// Starting velocity of each droplet.
        /// </summary>
        public readonly float initialVelocity;

        /// <summary>
        /// Force of gravity to pull droplets down gradient. Overall
        /// a higher gravity factor leads to faster erosion, but there are no differences in the
        /// appearance of the terrain.
        /// </summary>
        public readonly float gravity;

        /// <summary>
        /// Should velocity be included in computing a droplets carry capacity.
        /// </summary>
        public readonly bool includeVelocity;

        /// <summary>
        /// determines the amount of sediment a drop can carry as used
        /// in equation 5.4. A higher value results in more sediment being eroded on steeper
        /// ground and deposited in lower regions. Thus each drop has a higher impact on the 
        /// result. That leads to a rugged terrain with more ravines. 
        /// </summary>
        public readonly float sedimentCapacityFactor;

        /// <summary>
        /// determines how fast the drops evaporate. Again its value is 
        /// between 0 and 1. A faster evaporation leads to shorter paths of the drops in which they 
        /// influence the terrain.
        /// </summary>
        public readonly float evaporationRate;

        /// <summary>
        /// the minimum level of height difference that is taken for
        /// the calculation of the carry capacity of each single drop. Increasing the value ensures
        /// that the carry capacity does not fall below a certain line. Higher values lead to faster
        /// erosion but also stronger ravine forming. Terrains eroded with a low minimal slope
        /// need more drops but give smoother, more realistic looking terrains.
        /// </summary>
        public readonly float minSlope;

        /// <summary>
        /// Minimum capacity of a raindrop. It will always be able to hold at least this much sediment. 
        /// </summary>
        public readonly float minCapacity;

        /// <summary>
        /// Maximum time that a droplet can survive
        /// </summary>
        public readonly int maxDropletLifetime;

        /// <summary>
        /// limits the sediment that is dropped if the sediment carried
        /// by a drop exceeds the drops carry capacity. The value is
        /// between 0 and 1. Since the drop loses water over time through evaporation, it happens,
        /// that the capacity falls below the amount of currently carried sediment. For high values
        /// of deposition that leads to visible sediment deposition on the flow path
        /// </summary>
        public readonly float depositionRate;
        
        /// <summary>
        /// determines how much of the free
        /// capacity of a drop is filled with sediment in case of erosion.
        /// The value is between 0 and 1. With a high erosion speed, a drop quickly fills its
        /// capacity and after that most likely only deposits sediment. With a low value, the drops
        /// pick up sediment for a longer path, which results in stronger ravine formation.
        /// </summary>
        public readonly float erodeRate;

        /// <summary>
        /// determines the radius in which sediment is taken from the
        /// rock layer. The smaller radius is, the deeper and more distinct the ravines will be.
        /// Raising the erosion radius also increases the computational time needed for each drop
        /// drastically
        /// </summary>
        public readonly int erodeRadius;

        /// <summary>
        /// Amount of blur to use when applying erosion changes. Blurs change map before combining
        /// with regular map. Makes program significantly slower but does lead to smoother looking
        /// results. A blur is applied after a set of specified raindrops are created. The erosion
        /// map is blurred and the original erosion map and blurred map are combined in rations of
        /// original * (1 - blurValue) + blurred * (blurValue). Smoothing will look nicer when the
        /// program is run in larget batches.
        /// </summary>
        public readonly float blurValue;

        /// <summary>
        /// Size of blur filter. Larger blur filter will fade the changes more. This specific
        /// implementation uses a gaussian blur filter where the standard deviation
        /// is the size of the filter radius.
        /// </summary>
        public readonly int blurRadius;

        /// <summary>
        /// Brush for applying erode feature
        /// </summary>
        public readonly float[,] erodeBrush;

        /// <summary>
        /// Brush for blurring
        /// </summary>
        public readonly float[,] blurBrush;
    }
}