#ifndef __HEIGHTMAP_HLSL__
#define __HEIGHTMAP_HLSL__

/// <summary>
/// Height Map interface that can get and set the height at a specified x and y coordinate.
/// </summary>
interface iHeightMap {
    /// <summary>
    /// Gets the height of the map at a given x, y coordinate.
    /// </summary>
    /// <param name="pos">Coordinate in the grid with (x,y) components</param>
    /// <returns>Height at specified position as a float.</returns>
    float GetHeight(int2 pos);
    /// <summary>
    /// Adds to a map at the given coordinate by value of change
    /// </summary>
    /// <param name="pos">Coordinate in the grid with (x,y) components</param>
    /// <param name="change">Height to add at position x and y.</param>
    void AddHeight(int2 pos, float change);
    /// <summary>
    /// Checks if a coordinate is in the bounds of the heightmap.
    /// </summary>
    /// <param name="pos">Coordinate in the grid with (x,y) components</param>
    /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
    bool IsInBounds(int2 pos);
};

/// <summary>
/// Height Map interface that can get and set the height at a specified x and y coordinate.
/// </summary>
class cHeightMap : iHeightMap {
    /// <summary>
    /// Buffer for the height map that can be changed.
    /// </summary>
    RWStructuredBuffer<float> map;
    /// <summary>
    /// Size of the map (x and y components)
    /// </summary>
    int2 size;

    /// <summary>
    /// Gest the index in the height map of a specified location.
    /// </summary>
    /// <param name="pos">Coordinate in the grid with (x,y) components</param>
    /// <returns>The index in the height map that corresponds to the specified coordinates</returns>
    int GetIndex(int2 pos) {
        return pos.x + pos.y * size.y;
    }

    float GetHeight(int2 pos) {
        int2 bound = int2(min(max(0, pos.x), size.x - 1), min(max(0, pos.y), size.y - 1));
        return map[GetIndex(bound)];
    }

    void AddHeight(int2 pos, float change) {
        map[GetIndex(pos)] += change;
    }

    bool IsInBounds(int2 pos) {
        return pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y;
    }
};

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

    float GetHeight(int2 pos) {
        return mTop.GetHeight(pos) + mBot.GetHeight(pos);
    }

    void AddHeight(int2 pos, float change) {
        mTop.AddHeight(pos, change);
    }

    bool IsInBounds(int2 pos) {
        return mTop.IsInBounds(pos) && mBot.IsInBounds(pos);
    }
};

#endif // __HEIGHTMAP_HLSL__
