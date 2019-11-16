using UnityEngine;
using Terrain.Map;

namespace Terrain.Erosion {
    /// <summary>
    /// Class for utility functions to manage erosion
    /// </summary>
    public static class ErosionUtils {
        /// <summary>
        /// Compute the capacity of a droplet using factors like include velocity, min slope, and capacity
        /// factor. This says how much a droplet can carry.
        /// </summary>
        /// <param name="deltaH">Change in height from previous movement</param>
        /// <param name="velocity">Current velocity of droplet</param>
        /// <param name="waterFactor">Amount of water in droplet</param>
        /// /// <param name="parameters">Erosion parameters for controlling how erosion works</param>
        /// <returns>The computed capacity of the droplet or Minimum capacity fi it is less than
        /// than the computed value.</returns>
        public static float ComputeCapacity(float deltaH, float velocity, float waterFactor, HydroErosionParams parameters) {
            float slopeFactor = Mathf.Max(Mathf.Abs(deltaH), parameters.minSlope);
            float velFactor = Mathf.Max(1, parameters.includeVelocity ? velocity : 1);
            float capacity = slopeFactor * velFactor * waterFactor * parameters.sedimentCapacityFactor;
            return Mathf.Max(capacity, parameters.minCapacity);
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
        public static float DepositSediment(float deltaH, float sediment, float capacity,
            Vector2 pos, HeightMap map, HydroErosionParams parameters) {
            // Deposit all sediment if moving uphill (but not more than the size of the pit)
            float slopeBasedDeposit = Mathf.Min (deltaH, sediment);
            // Deposit proportion of sediment based on capacity
            float capacityBasedDeposit = (sediment - capacity) * parameters.depositionRate;
            // Select slope or capacity based on if moving uphill
            float amountToDeposit = (deltaH > 0) ? slopeBasedDeposit : capacityBasedDeposit;
            return ErosionUtils.Deposit(map, pos, amountToDeposit);
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
        public static float Erode(HeightMap map, Vector2 pos, float amountToErode, int radius, float[,] brush) {
            // Calculate the grid location (rounded down)
            int locX = (int) pos.x;
            int locY = (int) pos.y;
            
            float totalWeights = 0;
            float sd = radius / 3.0f;
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    if (!map.IsInBounds(x + locX, y + locY)) {
                        continue;
                    }
                    totalWeights += brush[x + radius, y + radius];
                }
            }

            float eroded = 0;
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    float weighedErodeAmount = brush[x + radius, y + radius] / totalWeights * amountToErode;
                    if (!map.IsInBounds(x + locX, y + locY)) {
                        continue;
                    }
                    float currentHeight = map.GetHeight(x + locX, y + locY);
                    float deltaSediment = (weighedErodeAmount > currentHeight) ? currentHeight : weighedErodeAmount;
                    map.AddHeight(x + locX, y + locY, -deltaSediment);
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
        public static float Deposit(HeightMap map, Vector2 pos, float amountToDeposit) {
            // Calculate the grid location (rounded down)
            int locX = (int) pos.x;
            int locY = (int) pos.y;

            // Find the offest in the X and Y axis from that location
            float offsetX = pos.x - locX;
            float offsetY = pos.y - locY;

            float deposited = 0;
            deposited += ErosionUtils.ChangeHeightMap(map, locX, locY, amountToDeposit * (1 - offsetX) * (1 - offsetY));
            deposited += ErosionUtils.ChangeHeightMap(map, locX + 1, locY, amountToDeposit * offsetX * (1 - offsetY));
            deposited += ErosionUtils.ChangeHeightMap(map, locX, locY + 1, amountToDeposit * (1 - offsetX) * offsetY);
            deposited += ErosionUtils.ChangeHeightMap(map, locX + 1, locY + 1, amountToDeposit * offsetX * offsetY);

            return deposited;
        }

        /// <summary>
        /// Approximates the height of a position using bilinear interpolation of a cell
        /// </summary>
        /// <param name="map">Height map to use</param>
        /// <param name="pos">Position of droplet</param>
        /// <returns>Weighted height by how close the position is to the edges of its cell</returns>
        public static float ApproximateHeight(HeightMap map, Vector2 pos) {
            // Calculate the grid location (rounded down)
            int locX = (int) pos.x;
            int locY = (int) pos.y;

            // Find the offest in the X and Y axis from that location
            float offsetX = pos.x - locX;
            float offsetY = pos.y - locY;

            // Calculate heights of the four nodes of the droplet's cell\
            float heightNW = map.GetHeight(locX, locY);
            float heightNE = map.GetHeight(locX + 1, locY);
            float heightSW = map.GetHeight(locX, locY + 1);
            float heightSE = map.GetHeight(locX + 1, locY + 1);

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
        public static Vector2 CalculateGradient(HeightMap map, Vector2 pos) {
            // Calculate the grid location (rounded down)
            int locX = (int) pos.x;
            int locY = (int) pos.y;

            // Find the offest in the X and Y axis from that location
            float offsetX = pos.x - locX;
            float offsetY = pos.y - locY;

            // Calculate heights of the four nodes of the droplet's cell\
            float heightNW = map.GetHeight(locX, locY);
            float heightNE = map.GetHeight(locX + 1, locY);
            float heightSW = map.GetHeight(locX, locY + 1);
            float heightSE = map.GetHeight(locX + 1, locY + 1);

            // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
            float gradientX = (heightNE - heightNW) * (1 - offsetY) + (heightSE - heightSW) * offsetY;
            float gradientY = (heightSW - heightNW) * (1 - offsetX) + (heightSE - heightNE) * offsetX;

            return new Vector2(gradientX, gradientY);
        }

        /// <summary>
        /// Sets value to the height map and add to a specific location. Will do nothing if the specified location
        /// is out of bounds.
        /// </summary>
        /// <param name="map">Map to apply changes to.</param>
        /// <param name="posX">X position on the map</param>
        /// <param name="posY">Y position on the map</param>
        /// <param name="value">Amount to add to the map</param>
        public static void SetHeightMap(HeightMap map, int posX, int posY, float value) {
            if (map.IsInBounds(posX, posY)) {
                map.SetHeight(posX, posY, value);
            }
        }

        /// <summary>
        /// Adds a value to the height map and add to a specific location. Will do nothing if the specified location
        /// is out of bounds.
        /// </summary>
        /// <param name="map">Map to apply changes to.</param>
        /// <param name="posX">X position on the map</param>
        /// <param name="posY">Y position on the map</param>
        /// <param name="change">Amount to add to the map</param>
        /// <returns>The amount added to the map. Will be zero if the location is out of bounds</returns>
        public static float ChangeHeightMap(HeightMap map, int posX, int posY, float change) {
            if (map.IsInBounds(posX, posY)) {
                map.AddHeight(posX, posY, change);
                return change;
            }
            return 0;
        }
    }
}