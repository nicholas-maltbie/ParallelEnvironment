using UnityEngine;
using System;

namespace Terrain.Map {
    /// <summary>
    /// Large Height map of a specified size
    /// </summary>
    [RequireComponent(typeof(AbstractHeightMapGenerator))]
    public class LargeHeightMap : MonoBehaviour, IHeightMap {

        /// <summary>
        /// Size of the map. Will be a square with edges of this length.
        /// </summary>
        public int mapSize = 1024;

        /// <summary>
        /// Minimum map height
        /// </summary>
        public int minHeight = 0;

        /// <summary>
        /// Maximum map height
        /// </summary>
        public int maxHeight = 256;

        /// <summary>
        /// Saved height map values.
        /// </summary>
        private float[] heightMap;
            

        /// <summary>
        /// Initialized the heightMap values using the HeightMapGenerator
        /// </summary>
        public void GenerateHeightMap() {
            AbstractHeightMapGenerator mapGen = GetComponent<AbstractHeightMapGenerator>();
            this.heightMap = mapGen.CreateHeightMap(this.mapSize);
            int heightMapRange = maxHeight - minHeight;

            for (int x = 0; x < this.mapSize; x++) {
                for (int y = 0; y < this.mapSize; y++) {
                    this.heightMap[GetMapIndex(x, y)] = heightMap[GetMapIndex(x, y)] * heightMapRange + minHeight;
                }
            }
        }

        /// <summary>
        /// Gets the height at a specified x, y location. If x and y are beyond the bounds of 
        /// the height map, the values are bounded to the closest edge.
        /// </summary>
        /// <param name="x">X coordinate in map</param>
        /// <param name="y">Y coordinate in map</param>
        /// <returns>Height saved at that specific x, y value. If x or y are < 0, the value less
        /// than zero is set to zero. If x or y are > mapSize - 1, then the value greater thn
        /// mapSize - 1 is set to mapSize - 1.</returns>
        public float GetHeight(int x, int y) {
            x = Mathf.Min(Mathf.Max(0, x), this.mapSize - 1);
            y = Mathf.Min(Mathf.Max(0, y), this.mapSize - 1);
            return this.heightMap[GetMapIndex(x, y)];
        }

        /// <summary>
        /// Gest the index in the height map of a specified location.
        /// </summary>
        /// <param name="x">X coordinate in the grid</param>
        /// <param name="y">Y coordinate in the grid</param>
        /// <returns>The index in the height map that corresponds to the specified coordinates</returns>
        private int GetMapIndex(int x, int y) {
            return x + y * this.mapSize;
        }

        /// <summary>
        /// Sets the height at a specified x and y. Will throw an ArgumentOutOfRangeExcpetion if 
        /// x and y do not fit the bounds 0 <= (x or y) < mapSize
        /// </summary>
        /// <param name="x">X coordinate in the grid</param>
        /// <param name="y">Y coordinate in the grid</param>
        /// <param name="height">Height of the map at specified location.</param>
        public void SetHeight(int x, int y, float height)
        {
            if (! IsInBounds(x, y)) 
                throw new ArgumentOutOfRangeException ();
            this.heightMap[GetMapIndex(x, y)] = height;
        }

        /// <summary>
        /// Checks if a location is in the bounds of the Large Height map
        /// </summary>
        /// <param name="x"> coodrinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <returns>True if x and y are in the bounds (greater than or equal to zero and less than mapSize)</returns>
        public bool IsInBounds(int x, int y) {
            return x >= 0 && x < this.mapSize && y >= 0 && y < this.mapSize;
        }

        /// <summary>
        /// Adds to the height at a specified x and y. Will throw an ArgumentOutOfRangeExcpetion if 
        /// x and y do not fit the bounds 0 <= (x or y) < mapSize
        /// </summary>
        /// <param name="x">X coordinate in the grid</param>
        /// <param name="y">Y coordinate in the grid</param>
        /// <param name="change">Height to add to the map at specified location.</param>
        public void AddHeight(int x, int y, float change)
        {
            if (! IsInBounds(x, y)) 
                throw new ArgumentOutOfRangeException ();
            this.heightMap[GetMapIndex(x, y)] += change;

        }
}
}
