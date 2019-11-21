# ifndef __HYDRO_EROSION_PARAMS_HLSL__
# define __HYDRO_EROSION_PARAMS_HLSL__

/// <summary>
/// Set of parameters for eroding the height map
/// </summary>
struct HydroErosionParams {
    /// <summary>
    /// Inertia value for how much momentum is preserved.
    /// </summary>
    float inertia;
    /// <summary>
    /// Starting water in each droplet.
    /// </summary>
    float initialWater;
    /// <summary>
    /// Starting velocity of each droplet.
    /// </summary>
    float initialVelocity;
    /// <summary>
    /// Force of gravity to pull droplets down gradient. Overall
    /// a higher gravity factor leads to faster erosion, but there are no differences in the
    /// appearance of the terrain.
    /// </summary>
    float gravity;
    /// <summary>
    /// Should velocity be included in computing a droplets carry capacity.
    /// </summary>
    bool includeVelocity;
    /// <summary>
    /// determines the amount of sediment a drop can carry as used
    /// in equation 5.4. A higher value results in more sediment being eroded on steeper
    /// ground and deposited in lower regions. Thus each drop has a higher impact on the 
    /// result. That leads to a rugged terrain with more ravines. 
    /// </summary>
    float sedimentCapacityFactor;
    /// <summary>
    /// determines how fast the drops evaporate. Again its value is 
    /// between 0 and 1. A faster evaporation leads to shorter paths of the drops in which they 
    /// influence the terrain.
    /// </summary>
    float evaporationRate;
    /// <summary>
    /// the minimum level of height difference that is taken for
    /// the calculation of the carry capacity of each single drop. Increasing the value ensures
    /// that the carry capacity does not fall below a certain line. Higher values lead to faster
    /// erosion but also stronger ravine forming. Terrains eroded with a low minimal slope
    /// need more drops but give smoother, more realistic looking terrains.
    /// </summary>
    float minSlope;
    /// <summary>
    /// Minimum capacity of a raindrop. It will always be able to hold at least this much sediment. 
    /// </summary>
    float minCapacity;
    /// <summary>
    /// Maximum time that a droplet can survive
    /// </summary>
    int maxDropletLifetime;
    /// <summary>
    /// limits the sediment that is dropped if the sediment carried
    /// by a drop exceeds the drops carry capacity. The value is
    /// between 0 and 1. Since the drop loses water over time through evaporation, it happens,
    /// that the capacity falls below the amount of currently carried sediment. For high values
    /// of deposition that leads to visible sediment deposition on the flow path
    /// </summary>
    float depositionRate;
    /// <summary>
    /// determines how much of the free
    /// capacity of a drop is filled with sediment in case of erosion.
    /// The value is between 0 and 1. With a high erosion speed, a drop quickly fills its
    /// capacity and after that most likely only deposits sediment. With a low value, the drops
    /// pick up sediment for a longer path, which results in stronger ravine formation.
    /// </summary>
    float erodeRate;
    /// <summary>
    /// determines the radius in which sediment is taken from the
    /// rock layer. The smaller radius is, the deeper and more distinct the ravines will be.
    /// Raising the erosion radius also increases the computational time needed for each drop
    /// drastically
    /// </summary>
    int erodeRadius;
    /// <summary>
    /// Amount of blur to use when applying erosion changes. Blurs change map before combining
    /// with regular map. Makes program significantly slower but does lead to smoother looking
    /// results. A blur is applied after a set of specified raindrops are created. The erosion
    /// map is blurred and the original erosion map and blurred map are combined in rations of
    /// original * (1 - blurValue) + blurred * (blurValue). Smoothing will look nicer when the
    /// program is run in large batches.
    /// </summary>
    float blurValue;
    /// <summary>
    /// Size of blur filter. Larger blur filter will fade the changes more. This specific
    /// implementation uses a gaussian blur filter where the standard deviation
    /// is the size of the filter radius.
    /// </summary>
    int blurRadius;
    /// <summary>
    /// Brush for applying erode feature
    /// </summary>
    StructuredBuffer<float> erodeBrush;
};

#endif // __HYDRO_EROSION_PARAMS_HLSL__
