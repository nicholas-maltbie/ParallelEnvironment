#ifndef __HEIGHTMAP_HLSL__
#define __HEIGHTMAP_HLSL__

/// <summary>
/// Height Map interface that can get and set the height at a specified x and y coordinate.
/// </summary>
interface iHeightMap {
    /// <summary>
    /// Gets the height of the map at a given x, y coordinate.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <returns>Height at specified position as a float.</returns>
    float GetHeight(int x, int y);
    /// <summary>
    /// Adds to a map at the given coordinate by value of change
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <param name="change">Height to add at position x and y.</param>
    void AddHeight(int x, int y, float change);
    /// <summary>
    /// Checks if a coordinate is in the bounds of the heightmap.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
    bool IsInBounds(int x, int y);
};

/// <summary>
/// Height Map interface that can get and set the height at a specified x and y coordinate.
/// </summary>
class cHeightMap : iHeightMap {
    /// <summary>
    /// Buffer for the height map that can be changed.
    /// </summary>cHeightMap
    RWStructuredBuffer<float> map;
    RWStructuredBuffer<uint> locks;
    /// <summary>
    /// Size of the map (x and y components)
    /// </summary>
    int2 size;

    /// <summary>
    /// Gest the index in the height map of a specified location.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <returns>The index in the height map that corresponds to the specified coordinates</returns>
    int GetIndex(int x, int y) {
        return x + y * size.x;
    }
    
    float GetHeight(int x, int y) {
        return map[GetIndex(min(max(0, x), size.x - 1), min(max(0, y), size.y - 1))];
    }

    void AddHeight(int x, int y, float change) {
        bool keepWaiting = true;
        int lockIdx = GetIndex(x, y);
        while(keepWaiting) {
            uint originalValue;
            InterlockedCompareExchange(locks[lockIdx], 0, 1, originalValue);
            if(originalValue == 0) {
                map[GetIndex(x, y)] += change;
                InterlockedExchange(locks[lockIdx], 0, originalValue);
                // exit loop
                keepWaiting = false;
            }
        }
    }

    bool IsInBounds(int x, int y) {
        return x >= 0 && x < size.x && y >= 0 && y < size.y;
    }
};

cHeightMap MakeHeightMap(RWStructuredBuffer<float> map, RWStructuredBuffer<uint> locks, int2 size) {
    cHeightMap dataMap;
    dataMap.map = map;
    dataMap.size = size;
    dataMap.locks = locks;
    return dataMap;
}

/// <summary>
/// Layered map is a combination of two cHeightMaps and allows for 
/// interacting with maps as a group.
/// </summary>
class cLayeredMap : iHeightMap {
    /// <summary>
    /// Top layer in the map that can be edited.
    /// </summary>
    cHeightMap mTop;
    /// <summary>
    /// Bottom layer in the map that cannot be changed.
    /// </summary>
    cHeightMap mBot;

    float GetHeight(int x, int y) {
        return mTop.GetHeight(x, y) + mBot.GetHeight(x, y);
    }

    void AddHeight(int x, int y, float change) {
        mTop.AddHeight(x, y, change);
    }

    bool IsInBounds(int x, int y) {
        return mTop.IsInBounds(x, y) &  mBot.IsInBounds(x, y);
    }
};

#endif // __HEIGHTMAP_HLSL__
# ifndef __DROPLET_HLSL__
# define __DROPLET_HLSL__

#include "Assets/Compute Shaders/Terrain/Erosion/HydroErosionParams.hlsl"
#include "Assets/Compute Shaders/Terrain/Erosion/ErosionUtils.hlsl"
#include "Assets/Compute Shaders/Terrain/Map/HeightMap.hlsl"

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
    int step;
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
    HydroErosionParams erodeParams;
    /// <summary>
    /// Height map for movement
    /// </summary>
    cLayeredMap layers;

    /// <summary>
    /// Checks if this droplet HasDied yet. Will die when it has taken too many steps,
    /// all its water has evaporated, or when it moves out of bounds of the map.
    /// The droplet is in a superposition of being both alive and dead until this
    /// method has been invoked. Just like the cat I left in that box... Oh No, 
    /// I'm coming for you Whiskers!
    /// </summary>
    /// <returns>True if the droplet has died, false if it is still live.</returns>
    bool HasDied() {
        bool tooOld = step > erodeParams.maxDropletLifetime;
        bool outOfBounds = !layers.IsInBounds((int)pos.x, (int)pos.y);
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
        float slopeFactor = max(abs(deltaH), erodeParams.minSlope);
        float velFactor = max(1, erodeParams.includeVelocity ? vel : 1);
        float capacity = slopeFactor * velFactor * water * erodeParams.sedimentCapacityFactor;
        return max(min(abs(deltaH), capacity), erodeParams.minCapacity);
    }

    /// <summary>
    /// Complete a single step in the droplets movement and erode/deposit on map if needed.
    /// </summary>
    void Simulate() {
        for (step = 0; step < erodeParams.maxDropletLifetime; step++) {
            float2 grad = CalculateGradient(layers, pos.x, pos.y);

            dir = dir * erodeParams.inertia - grad * (1 - erodeParams.inertia);
            float magnitude = sqrt(dir.x * dir.x + dir.y * dir.y);
            dir /= magnitude;

            float2 posNew = pos + dir;

            // Calculate the change in height
            float heightOld = ApproximateHeight(layers, pos);
            float heightNew = ApproximateHeight(layers, posNew);
            float deltaH = heightNew - heightOld;

            float capacity = ComputeCapacity(deltaH);

            // if droplet moved off the map or stopped moving, kill it
            if (water == 0 || !layers.IsInBounds(floor(posNew.x), floor(posNew.y))) {
                sediment -= DepositSediment(layers, deltaH, sediment, capacity, pos, erodeParams);
                pos = posNew;
                step = erodeParams.maxDropletLifetime;
                continue;
            }
            
            // If the droplet is carrying too much sediment, it will drop its sediment
            if (deltaH >= 0 || sediment > capacity) {
                sediment -= DepositSediment(layers, deltaH, sediment, capacity, pos, erodeParams);
            }
            // If the droplet is following downhill and has excess capacity, it will erode terrain
            else {
                float amountToErode = min((capacity - sediment) * erodeParams.erodeRate, -deltaH);
                sediment += Erode(layers, pos, amountToErode, erodeParams.erodeRadius, erodeParams.erodeBrush);
            }

            pos = posNew;
            water *= (1 - erodeParams.evaporationRate);
            vel = sqrt(max(0, vel * vel + -deltaH * erodeParams.gravity));
        }
    }
};

# endif //__DROPLET_HLSL__
