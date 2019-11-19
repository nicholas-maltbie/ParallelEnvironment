# ifndef __DROPLET_HLSL__
# define __DROPLET_HLSL__

#include "Assets/Shaders/Terrain/Erosion/HydroErosionParams.hlsl"
#include "Assets/Shaders/Terrain/Erosion/ErosionUtils.hlsl"
#include "Assets/Shaders/Terrain/Map/HeightMap.hlsl"

/// <summary>
/// A droplet has the ability to move along a map and erode or deposit
/// terrain depending on what has happened on the map.
/// </summary>
class Droplet {
    /// <summary>
    /// Position of the droplet on the map
    /// </summary>
    float2 pos;
    /// <summary>
    /// Amount of water that the droplet is carrying
    /// </summary>
    float water;
    /// <summary>
    /// Current speed of the droplet
    /// </summary>
    float speed;
    /// <summary>
    /// Velocity of the droplet
    /// </summary>
    float vel;
    /// <summary>
    /// Number of steps that the droplet has taken
    /// </summary>
    int steps;
    /// <summary>
    /// Current direction of movement of the droplet
    /// </summary>
    float2 dir;
    /// <summary>
    /// Current amount of sediment that the droplet is carrying
    /// </summary>
    float sediment;
    /// <summary>
    /// Parameters for controlling droplet behaviour
    /// </summary>
    HydroErosionParams erosionParams;
    /// <summary>
    /// Height map for movement
    /// </summary>
    cLayeredMap map;
};

# endif //__DROPLET_HLSL__