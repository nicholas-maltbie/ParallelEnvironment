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
    /// </summary>
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
