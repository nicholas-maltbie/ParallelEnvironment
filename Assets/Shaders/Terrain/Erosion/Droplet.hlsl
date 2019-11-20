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
    RWStructuredBuffer<float> debug;

    /// <summary>
    /// Checks if this droplet HasDied yet. Will die when it has taken too many steps,
    /// all its water has evaporated, or when it moves out of bounds of the map.
    /// The droplet is in a superposition of being both alive and dead until this
    /// method has been invoked. Just like the cat I left in that box... Oh No, 
    /// I'm coming for you Whiskers!
    /// </summary>
    /// <returns>True if the droplet has died, false if it is still live.</returns>
    bool HasDied() {
        bool tooOld = steps > erosionParams.maxDropletLifetime;
        bool outOfBounds = !map.IsInBounds((int)pos.x, (int)pos.y);
        bool outOfWater = water == 0;
        return tooOld || outOfBounds || outOfWater;
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
    float ComputeCapacity(float deltaH) {
        float slopeFactor = max(abs(deltaH), erosionParams.minSlope);
        float velFactor = max(1, erosionParams.includeVelocity ? vel : 1);
        float capacity = slopeFactor * velFactor * water * erosionParams.sedimentCapacityFactor;
        return max(capacity, erosionParams.minCapacity);
    }

    /// <summary>
    /// Complete a single step in the droplets movement and erode/deposit on map if needed.
    /// </summary>
    void DoStep() {
        // increment steps by one
        steps += 1;
        debug[0] = pos.x;
        debug[1] = pos.y;

        // Compute gradient at current position
        float2 grad = CalculateGradient(map, pos.x, pos.y);
        debug[2] = grad.x;
        debug[3] = grad.y;

        // Compute new direction as combination of old direction and gradient
        // Add some inertia for fun
        dir = dir * erosionParams.inertia - grad.x * (1 - erosionParams.inertia);

        // Select a random direction if dir is zero
        if (dir.x == 0 && dir.y == 0) {
            dir = float2(0, 0);
        }

        // Normalize the vector dir so that it only moves on cell
        // at a time. This stops raindrops from skipping areas of the map.
        float mag = sqrt(dir.x * dir.x + dir.y * dir.y);
        dir = float2(dir.x / mag, dir.y / mag);
        debug[4] = dir.x;
        debug[5] = dir.y;

        // Calculate the new position
        float2 posNew = float2(pos.x + dir.x, pos.y + dir.y);

        // Calculate the change in height
        float heightOld = ApproximateHeight(map, pos);
        float heightNew = ApproximateHeight(map, posNew);
        float deltaH = heightNew - heightOld;

        // Calculate the carrying capacity of the droplet
        float capacity = ComputeCapacity(deltaH);

        // if droplet moved off the map or stopped moving, kill it
        if (water == 0 || !map.IsInBounds((int)posNew.x, (int)posNew.y)) {
            pos = posNew;
            return;
        }
        
        // If the droplet is carrying too much sediment, it will drop its sediment
        if (deltaH >= 0 || sediment > capacity) {
            //sediment -= DepositSediment(map, deltaH, sediment, capacity, pos, erosionParams);
        }
        // If the droplet is following downhill and has excess capacity, it will erode terrain
        else {
            //float amountToErode = min((capacity - sediment) * erosionParams.erodeRate, -deltaH);
            //sediment += amountToErode;
            //sediment += Erode(map, pos, amountToErode, erosionParams.erodeRadius, erosionParams.erodeBrush);
        }

        // Update velocity
        vel = sqrt(max(0, vel * vel + -deltaH * erosionParams.gravity));
        // Updater water
        water = water * (1 - erosionParams.evaporationRate);
        // Update position
        pos = posNew;
    }
};

# endif //__DROPLET_HLSL__