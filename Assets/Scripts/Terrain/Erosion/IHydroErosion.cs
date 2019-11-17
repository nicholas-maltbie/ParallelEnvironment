using UnityEngine;
using System;
using Terrain.Map;

namespace Terrain.Erosion {
    /// <summary>
    /// Simulated hydraulic erosion following 
    /// Hans Theobald Beyer's "Implementation of a method for Hydraulic Erosion"
    /// Bachelor's Thesis in Informatics at TECHNISCHE UNIVERSITÄT MÜNCHEN. 15.11.2015
    /// 
    /// https://www.firespark.de/resources/downloads/implementation%20of%20a%20methode%20for%20hydraulic%20erosion.pdf
    /// 
    /// Some of the documentation of parameters is directly from this document so if
    /// there is any uncertainty there is further explanation in this paper.
    /// 
    /// Simulates Hydraulic erosion by spawning raindrops around the map. As the raindrops
    /// move down the sides of the terrain they will erode sides of the mountian.
    /// The raindrop's movement at each individual step will be one cell in the (x,y) direction.
    /// If the raindrop is moving downhill quickly it will pick up sediment.
    /// As the raindrop picks up sediment, it will fill up and slowly deposit sediment if it
    /// becomes too full or slows down enough. 
    /// The raindrop will also slowly lose water and will die if it moves off the map
    /// or runs out of water completely. 
    /// </summary>
    public interface IHydroErosion {
        /// <summary>
        /// Erodes a hight map by generating a set of droplets then simulating their movement along the height map.
        /// This will NOT change the height map. It only returns the set of changes that should 
        /// be applied or blurred.
        /// </summary>
        /// <param name="map">Map to apply changes to.</param>
        /// <param name="start">Minimum location for spawning droplets (X,Y) position</param>
        /// <param name="end">Maximum location for spawning droplets (X,Y) position</param>
        /// <param name="iterations">Number of droplets to create</param>
        /// <param name="erosionParams">Parameters for erosion</param>
        /// <param name="prng">Random number generator for droplet spawning and decisions</param>
        /// <returns>A change map of all the changes that need to be made to the map.
        /// This change map should be applied to the original height map.</returns>
        IChangeMap DoErosion(IHeightMap map, Vector2Int start, Vector2Int end, int iterations,
            HydroErosionParams erosionParams, System.Random prng);

    }
}
