# ifndef __EROSION_UTILS_HLSL__
# define __EROSION_UTILS_HLSL__

#include "Assets/Shaders/Terrain/Map/HeightMap.hlsl"

/// <summary>
/// Adds a value to the height map and add to a specific location. Will do nothing if the specified location
/// is out of bounds.
/// </summary>
/// <param name="map">Map to apply changes to.</param>
/// <param name="pos">position on the map with (x,y) components</param>
/// <param name="change">Amount to add to the map</param>
/// <returns>The amount added to the map. Will be zero if the location is out of bounds</returns>
float ChangeHeightMap(iHeightMap map, int2 pos, float change) {
    if (map.IsInBounds(pos)) {
        map.AddHeight(pos, change);
        return change;
    }
    return 0;
}

/// <summary>
/// Erodes a height map at a given position using a erosion brush.
/// </summary>
/// <param name="map">Map to change</param>
/// <param name="pos">Position of droplet on map</param>
/// <param name="amountToErode">Total amount to be removed</param>
/// <param name="radius">Radius of the brush</param>
/// <param name="brush">Brush to use when applying erosion</param>
/// <returns>The total amount of soil eroded (might be slightly less than amountToErode</returns>
float Erode(iHeightMap map, float2 pos, float amountToErode, int radius, StructuredBuffer<float> brush) {
    // Calculate the grid location (rounded down)
    int2 loc = int2(floor(pos.x), floor(pos.y));
    
    int brushSize = radius * 2 + 1;
    
    float totalWeights = 0;
    float sd = radius / 3.0f;
    for (int x_w = -radius; x_w <= radius; x_w++) {
        for (int y = -radius; y <= radius; y++) {
            if (!map.IsInBounds(int2(x_w + loc.x, y + loc.y))) {
                continue;
            }
            totalWeights += brush[x_w + radius + ((y + radius) * brushSize)];
        }
    }

    float eroded = 0;
    for (int x = -radius; x <= radius; x++) {
        for (int y = -radius; y <= radius; y++) {
            float weighedErodeAmount = brush[x + radius + ((y + radius) * brushSize)] / totalWeights * amountToErode;
            if (!map.IsInBounds(int2(x + loc.x, y + loc.y))) {
                continue;
            }
            float currentHeight = map.GetHeight(int2(x + loc.x, y + loc.y));
            float deltaSediment = weighedErodeAmount;
            if (weighedErodeAmount > currentHeight) {
                deltaSediment = currentHeight;
            }
            printf(x);
            map.AddHeight(int2(x + loc.x, y + loc.y), -deltaSediment);
            eroded += deltaSediment;
        }
    }
    return eroded;

}

/// <summary>
/// Deposit an amount of sediment on the map at a given location. Uses BiLinear interpolation to deposit
/// a proportional amount of soil at each corner of the cell in the height map.
/// </summary>
/// <param name="map">Map to change</param>
/// <param name="pos">Position of the droplet</param>
/// <param name="amountToDeposit">Amount of soil to add</param>
/// <returns>The total amount of soil deposited. Might be slightly less if parts of the cell are outside 
/// of the grid.</returns>
float Deposit(iHeightMap map, float2 pos, float amountToDeposit) {
    // Calculate the grid location (rounded down)
    int2 loc = int2((int) pos.x, (int) pos.y);

    // Find the offest in the X and Y axis from that location
    float offsetX = pos.x - loc.x;
    float offsetY = pos.y - loc.y;

    float deposited = 0;
    deposited += ChangeHeightMap(map, loc, amountToDeposit * (1 - offsetX) * (1 - offsetY));
    deposited += ChangeHeightMap(map, int2(loc.x + 1, loc.y), amountToDeposit * offsetX * (1 - offsetY));
    deposited += ChangeHeightMap(map, int2(loc.x, loc.y + 1), amountToDeposit * (1 - offsetX) * offsetY);
    deposited += ChangeHeightMap(map, int2(loc.x + 1, loc.y + 1), amountToDeposit * offsetX * offsetY);

    return deposited;
}

/// <summary>
/// Evaluate how much sediment is deposited and deposit that much
/// sediment based on the droplets current state.
/// </summary>
/// <param name="deltaH">Change in height since last move</param>
/// <param name="sediment">Amount of sediment current held in the droplet</param>
/// <param name="capacity">Capacity of the current droplet</param>
/// <param name="pos">Position of the current droplet</param>
/// <param name="map">Map to read and make changes to</param>
/// <param name="parameters">Erosion parameters for controlling how erosion works</param>
/// <returns>Actual amount of sediment deposited</returns>
float DepositSediment(iHeightMap map, float deltaH, float sediment, float capacity,
    float2 pos, HydroErosionParams parameters) {
    // Deposit all sediment if moving uphill (but not more than the size of the pit)
    float slopeBasedDeposit = min (deltaH, sediment);
    // Deposit proportion of sediment based on capacity
    float capacityBasedDeposit = (sediment - capacity) * parameters.depositionRate;
    // Select slope or capacity based on if moving uphill
    float amountToDeposit = (deltaH > 0) ? slopeBasedDeposit : capacityBasedDeposit;
    return Deposit(map, pos, amountToDeposit);
}

/// <summary>
/// Approximates the height of a position using bilinear interpolation of a cell
/// </summary>
/// <param name="map">Height map to use</param>
/// <param name="pos">Position of droplet</param>
/// <returns>Weighted height by how close the position is to the edges of its cell</returns>
float ApproximateHeight(iHeightMap map, float2 pos) {
    // Calculate the grid location (rounded down)
    int2 loc = int2((int) pos.x, (int) pos.y);

    // Find the offest in the X and Y axis from that location
    float offsetX = pos.x - loc.x;
    float offsetY = pos.y - loc.y;

    // Calculate heights of the four nodes of the droplet's cell
    float heightNW = map.GetHeight(int2(loc.x, loc.y));
    float heightNE = map.GetHeight(int2(loc.x + 1, loc.y));
    float heightSW = map.GetHeight(int2(loc.x, loc.y + 1));
    float heightSE = map.GetHeight(int2(loc.x + 1, loc.y + 1));

    return 
        heightNW * (1 - offsetX) * (1 - offsetY) +
        heightNE * offsetX * (1 - offsetY) +
        heightSW * (1 - offsetX) * offsetY +
        heightSE * offsetX * offsetY;
}

/// <summary>
/// Calculates the gradient of a map at a given position. Uses BiLinear interpolation to guess
/// the actual height between grid cells.
/// </summary>
/// <param name="map">Map with height information.</param>
/// <param name="pos">X,Y position on the map.</param>
/// <returns>A BiLinear interpolation of the height at a given x and y position.</returns>
float2 CalculateGradient(iHeightMap map, float2 pos) {
    // Calculate the grid location (rounded down)
    int2 loc = int2((int) pos.x, (int) pos.y);

    // Find the offest in the X and Y axis from that location
    float offsetX = pos.x - loc.x;
    float offsetY = pos.y - loc.y;

    // Calculate heights of the four nodes of the droplet's cell
    float heightNW = map.GetHeight(int2(loc.x, loc.y));
    float heightNE = map.GetHeight(int2(loc.x + 1, loc.y));
    float heightSW = map.GetHeight(int2(loc.x, loc.y + 1));
    float heightSE = map.GetHeight(int2(loc.x + 1, loc.y + 1));

    // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
    float gradientX = (heightNE - heightNW) * (1 - offsetY) + (heightSE - heightSW) * offsetY;
    float gradientY = (heightSW - heightNW) * (1 - offsetX) + (heightSE - heightNE) * offsetX;

    return float2(gradientX, gradientY);
}

#endif // __EROSION_UTILS_HLSL__
