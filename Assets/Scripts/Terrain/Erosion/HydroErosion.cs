using UnityEngine;
using System;

/// <summary>
/// Simulated hydraulic erosion following 
/// Hans Theobald Beyer's "Impleemtantion of a method for Hydraulic Erosion"
/// Bachelor's Thesis in Informatics at TECHNISCHE UNIVERSITÄT MÜNCHEN. 15.11.2015
/// 
/// https://www.firespark.de/resources/downloads/implementation%20of%20a%20methode%20for%20hydraulic%20erosion.pdf
/// 
/// Some of the documentation of parameters is direclty from this document so if
/// there is any uncertianty there is further explanation in this paper.
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
public class HydroErosion : MonoBehaviour {
    /// <summary>
    /// Seed value for random number generator
    /// </summary>
    public int seed = 0;

    /// <summary>
    /// Inertia value for how much momentum is preserved.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float inertia = 0.05f;

    /// <summary>
    /// Starting water in each droplet.
    /// </summary>
    public float initialWater = 1;
    /// <summary>
    /// Starting velocity of each droplet.
    /// </summary>
    public float initialVelocity = 1;

    /// <summary>
    /// Force of gravity to pull droplets down gradient. Overall
    /// a higher gravity factor leads to faster erosion, but there are no differences in the
    /// appearance of the terrain.
    /// </summary>
    public float gravity = 9.81f;

    /// <summary>
    /// Should velocity be included in computing a droplets carry capacity.
    /// </summary>
    public bool includeVelocity = false;

    /// <summary>
    /// determines the amount of sediment a drop can carry as used
    /// in equation 5.4. A higher value results in more sediment being eroded on steeper
    /// ground and deposited in lower regions. Thus each drop has a higher impact on the 
    /// result. That leads to a ruggy terrain with more ravines. 
    /// </summary>
    public float sedimentCapacityFactor = 4;

    /// <summary>
    /// determines how fast the drops evaporate. Again its value is 
    /// between 0 and 1. A faster evaporation leads to shorter paths of the drops in which they 
    /// influence the terrain.
    /// </summary>
    public float evaporationRate = 0.0123f;

    /// <summary>
    /// the minimum level of height difference that is taken for
    /// the calculation of the carry capacity of each single drop. Increasing the value ensures
    /// that the carry capacity does not fall below a certain line. Higher values lead to faster
    /// erosion but also stronger ravine forming. Terrains eroded with a low minimal slope
    /// need more drops but give smoother, more realistic looking terrains.
    /// </summary>
    public float minSlope = 0.01f;

    /// <summary>
    /// Minimum capacity of a raindrop. It will always be able to hold at least this much sediment. 
    /// </summary>
    public float minCapacity = 0.1f;

    /// <summary>
    /// Maximum time that a droplet can survive
    /// </summary>
    public int maxDropletLifetime = 256;

    /// <summary>
    /// limits the sediment that is dropped if the sediment carried
    /// by a drop exceeds the drops carry capacity c as described in equation 5.5. The value is
    /// between 0 and 1. Since the drop loses water over time through evaporation, it happens,
    /// that the capacity falls below the amount of currently carried sediment. For high values
    /// of pdeposition that leads to visible sediment deposition on the flow path
    /// </summary>
    public float depositionRate = 0.1f;
    
    /// <summary>
    /// determines how much of the free
    /// capacity of a drop is filled with sediment in case of erosion as described in equation
    /// 5.6. The value is between 0 and 1. With a high erosion speed, a drop quickly fills its
    /// capacity and after that most likely only deposits sediment. With a low value, the drops
    /// pick up sediment for a longer path, which results in stronger ravine formation.
    /// </summary>
    public float erosionRate = 0.1f;

    /// <summary>
    /// determines the radius in which sediment is taken from the
    /// rock layer. The smaller pradius is, the deeper and more distinct the ravines will be.
    /// Raising the erosion radius also increases the computational time needed for each drop
    /// drastically
    /// </summary>
    public int erodeRadius = 3;
    /// <summary>
    /// Amount of blur to use when applying erosion changes. Blurs change map before combining
    /// with regular map. Makes program significantly slower but does lead to smoother looking
    /// reuslts. A blur is applied after a set of specified raindrops are created. The erosion
    /// map is blurred and the original erosion map and blurred map are combined in rations of
    /// original * (1 - blurValue) + blurred * (blurValue). Smoothing will look nicer when the
    /// program is run in larget batches.
    /// </summary>
    [Range(0, 1)]
    public float blurValue = 0f;
    /// <summary>
    /// Size of blur filter. Larger blur filts will fade the changes more.
    /// </summary>
    public int blurRadius = 2;

    /// <summary>
    /// Pseudo-random number generator for behaviour of raindrops
    /// </summary>
    private System.Random prng;

    /// <summary>
    /// Brush for applying erode feature
    /// </summary>
    private float[,] erodeBrush;
    
    /// <summary>
    /// Brush to apply to deposit locations.
    /// </summary>
    private float[,] depositBrush;

    /// <summary>
    /// Brush for blurring
    /// </summary>
    private float[,] blurBrush;

    /// <summary>
    /// Initializes the erosion brush and prng
    /// </summary>
    /// <param name="seed">Seed value for PRNG, zero means use arbitrary seed</param>
    /// <param name="erodeRadius">Radius size of erosion brush</param>
    private void Initialize() {
        // setup PRNG
        if (this.prng == null) {
            this.prng = this.seed == 0 ? new System.Random() : new System.Random(seed);
        }

        // setup erode brush
        if (this.erodeBrush == null) {
            this.erodeBrush = new float[this.erodeRadius * 2 + 1, this.erodeRadius * 2 + 1];
            float sd = this.erodeRadius;
            for (int x = -this.erodeRadius; x <= this.erodeRadius; x++) {
                for (int y = -this.erodeRadius; y <= this.erodeRadius; y++) {
                    this.erodeBrush[x + this.erodeRadius,y + this.erodeRadius] = 
                        Mathf.Exp(- (x * x + y * y) / (2.0f * sd * sd)) / Mathf.Sqrt(2.0f * Mathf.PI * sd * sd);
                }
            }
        }

        // setup blur brush
        if (this.blurBrush == null) {
            this.blurBrush = new float[this.blurRadius * 2 + 1, this.blurRadius * 2 + 1];
            float sd = this.blurRadius;
            for (int x = -this.blurRadius; x <= this.blurRadius; x++) {
                for (int y = -this.blurRadius; y <= this.blurRadius; y++) {
                    this.blurBrush[x + this.blurRadius,y + this.blurRadius] =
                        Mathf.Exp(- (x * x + y * y) / (2.0f * sd * sd)) / Mathf.Sqrt(2.0f * Mathf.PI * sd * sd);
                }
            }
        }
    }

    /// <summary>
    /// Compute the capacity of a droplet using factors like include velocity, min slope, and capacity
    /// factor. This says how much a droplet can carry.
    /// </summary>
    /// <param name="deltaH">Change in sope from previous movement</param>
    /// <param name="velocity">Current velocity of droplet</param>
    /// <param name="waterFactor">Amount of water in droplet</param>
    /// <returns>The computed capacity of the droplet or Minimum capacity fi it is less than
    /// than the computed value.</returns>
    private float ComputeCapacity(float deltaH, float velocity, float waterFactor) {
        float slopeFactor = Mathf.Max(Mathf.Abs(deltaH), this.minSlope);
        float velFactor = Mathf.Max(1, this.includeVelocity ? velocity : 1);
        float capacity = slopeFactor * velFactor * waterFactor * this.sedimentCapacityFactor;
        return Math.Max(capacity, this.minCapacity);
    }

    /// <summary>
    /// Erodes a hight map by generating a set of droplets then simulating their movement along the height map.
    /// </summary>
    /// <param name="map">Map to apply changes to.</param>
    /// <param name="startX">Minimum location for spawning droplets (X axis)</param>
    /// <param name="startY">Minimum location for spawning droplets (Y axis)</param>
    /// <param name="endX">Maximum location for spawning droplets (X axis)</param>
    /// <param name="endY">Maximum location for spawning droplets (Y axis)</param>
    /// <param name="iterations">Number of droplets to create</param>
    public void ErodeHeightMap(HeightMap heightMap, int startX, int startY, int endX, int endY, int iterations) {
        Initialize();

        // Map for chanes in current set of raindrops
        ChangeMap deltaMap = new ChangeMap(endX - startX, endY - startY);
        // Layered map for storing information about the original map and delta map together
        LayeredMap layers = new LayeredMap(deltaMap, heightMap);
        
        // Iteration for each raindrop
        for (int iter = 0; iter < iterations; iter++) {

            // Simulate each raindrop
            // Put the raindrop at a random position in the grid
            Vector2 pos = new Vector2(prng.Next(startX + 1, endX - 1), prng.Next(startY + 1, endY - 1));
            // Previous direction the droplet moved in
            Vector2 dir = Vector2.zero;

            float sediment = 0;
            float water = this.initialWater;
            float vel = this.initialVelocity;

            for (int totalSteps = 0; totalSteps < this.maxDropletLifetime; totalSteps++) {
                // Compute gradient at current position
                Vector2 grad = CalculateGradient(layers, pos);

                // Compute new direction as combination of old direction and gradient
                // Add some intertia for fun
                dir = dir * this.inertia - grad * (1 - this.inertia);

                // Select a random direction if dir is zero
                if (dir.x == 0 && dir.y == 0) {
                    dir = new Vector2(prng.Next(), prng.Next());
                }

                // Normalize the vector dir so that it only moves on cell
                // at a time. This stops raindrops from skipping areas of the map.
                dir /= dir.magnitude;

                // Calculate the new position
                Vector2 posNew = pos + dir;

                // Calculate the chnage in height
                float heightOld = ApproximateHeight(layers, pos);
                float heightNew = ApproximateHeight(layers, posNew);
                float deltaH = heightNew - heightOld;

                // Calculate the carying capacity of the droplet
                float capacity = ComputeCapacity(deltaH, vel, water);

                // if droplet moved off the map or stopped moving, kill it
                if (water == 0) {
                    // Don't deposit because this should have already been handled
                    break;
                }
                else if (!layers.IsInBounds((int) posNew.x, (int) posNew.y)) {
                    // If the droplet had excess sediment, attempt to deposit it.
                    if (sediment > 0) {
                        float amountToDeposit = (deltaH > 0) ? Mathf.Min (deltaH, sediment) : (sediment - capacity) * this.depositionRate;
                        sediment -= Deposit(layers, pos, amountToDeposit);
                    }
                    break;
                }

                // If the droplet is carying too much sediment, it will drop its sediment
                if (deltaH >= 0 || sediment > capacity) {
                    float amountToDeposit = (deltaH > 0) ? Mathf.Min (deltaH, sediment) : (sediment - capacity) * this.depositionRate;
                    sediment -= amountToDeposit;
                    Deposit(layers, pos, amountToDeposit);
                }
                // If the droplet is flowign downhill and has excess capacity, it will erode terrain
                else {
                    float amountToErode = Mathf.Min((capacity - sediment) * this.erosionRate, -deltaH);
                    sediment += amountToErode;
                    Erode(layers, pos, amountToErode, this.erodeRadius, this.erodeBrush);
                }

                // Update velocity
                vel = Mathf.Sqrt(Mathf.Max(0, vel * vel + -deltaH * this.gravity));
                // Updater water
                water = water * (1 - this.evaporationRate);
                // Update position
                pos = posNew;
            }
        }

        // If bluring changes, do steps to blur map
        if (this.blurValue > 0) {
            // Calculate the blurred map by applying the blur brush kernel to the map
            ChangeMap blurredMap = deltaMap.ApplyKernel(blurBrush);
            // Multipley the original map and blurred map by ratios
            blurredMap.Multiply(this.blurValue);
            deltaMap.Multiply(1 - this.blurValue);

            // Apply changes to the original height map
            blurredMap.ApplyChangesToMap(heightMap);
            deltaMap.ApplyChangesToMap(heightMap);
        }
        // If not bluring changes, just ignore that complexity
        else {
            deltaMap.ApplyChangesToMap(heightMap);
        }
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
    private float Erode(HeightMap map, Vector2 pos, float amountToErode, int radius, float[,] brush) {
        // Calculate the grid location (rounded down)
        int coordX = (int) pos.x;
        int coordY = (int) pos.y;
        
        float totalWeights = 0;
        float sd = radius / 3.0f;
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                if (!map.IsInBounds(x + coordX, y + coordY)) {
                    continue;
                }
                totalWeights += brush[x + radius, y + radius];
            }
        }

        float eroded = 0;
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                float weighedErodeAmount = brush[x + radius, y + radius] / totalWeights * amountToErode;
                if (!map.IsInBounds(x + coordX, y + coordY)) {
                    continue;
                }
                float deltaSediment = (map.GetHeight(x + coordX, y + coordY) < weighedErodeAmount) ? map.GetHeight(x + coordX, y + coordY) : weighedErodeAmount;
                map.AddHeight(x + coordX, y + coordY, -deltaSediment);
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
    private float Deposit(HeightMap map, Vector2 pos, float amountToDeposit) {
        // Calculate the grid location (rounded down)
        int coordX = (int) pos.x;
        int coordY = (int) pos.y;

        // Find the offest in the X and Y axis from that location
        float offsetX = pos.x - coordX;
        float offsetY = pos.y - coordY;

        float deposited = 0;
        deposited += ChangeHeightMap(map, coordX, coordY, amountToDeposit * (1 - offsetX) * (1 - offsetY));
        deposited += ChangeHeightMap(map, coordX + 1, coordY, amountToDeposit * offsetX * (1 - offsetY));
        deposited += ChangeHeightMap(map, coordX, coordY + 1, amountToDeposit * (1 - offsetX) * offsetY);
        deposited += ChangeHeightMap(map, coordX + 1, coordY + 1, amountToDeposit * offsetX * offsetY);

        return deposited;
    }

    /// <summary>
    /// Approximates the height of a position using bilinear interpolation of a cell
    /// </summary>
    /// <param name="map">Height map to use</param>
    /// <param name="pos">Position of droplet</param>
    /// <returns>Weighted height by how close the position is to the edges of its cell</returns>
    private float ApproximateHeight(HeightMap map, Vector2 pos) {
        // Calculate the grid location (rounded down)
        int coordX = (int) pos.x;
        int coordY = (int) pos.y;

        // Find the offest in the X and Y axis from that location
        float offsetX = pos.x - coordX;
        float offsetY = pos.y - coordY;

        // Calculate heights of the four nodes of the droplet's cell\
        float heightNW = map.GetHeight(coordX, coordY);
        float heightNE = map.GetHeight(coordX + 1, coordY);
        float heightSW = map.GetHeight(coordX, coordY + 1);
        float heightSE = map.GetHeight(coordX + 1, coordY + 1);

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
    private Vector2 CalculateGradient(HeightMap map, Vector2 pos) {
        // Calculate the grid location (rounded down)
        int coordX = (int) pos.x;
        int coordY = (int) pos.y;

        // Find the offest in the X and Y axis from that location
        float offsetX = pos.x - coordX;
        float offsetY = pos.y - coordY;

        // Calculate heights of the four nodes of the droplet's cell\
        float heightNW = map.GetHeight(coordX, coordY);
        float heightNE = map.GetHeight(coordX + 1, coordY);
        float heightSW = map.GetHeight(coordX, coordY + 1);
        float heightSE = map.GetHeight(coordX + 1, coordY + 1);

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
    /// <param name="posx">X position on the map</param>
    /// <param name="posy">Y position on the map</param>
    /// <param name="value">Amount to add to the map</param>
    private void SetHeightMap(HeightMap map, int posx, int posy, float value) {
        if (map.IsInBounds(posx, posy)) {
            map.SetHeight(posx, posy, value);
        }
    }

    /// <summary>
    /// Adds a value to the height map and add to a specific location. Will do nothing if the specified location
    /// is out of bounds.
    /// </summary>
    /// <param name="map">Map to apply changes to.</param>
    /// <param name="posx">X position on the map</param>
    /// <param name="posy">Y position on the map</param>
    /// <param name="change">Amount to add to the map</param>
    /// <returns>The amount added to the map. Will be zero if the location is out of bounds</returns>
    private float ChangeHeightMap(HeightMap map, int posx, int posy, float change) {
        if (map.IsInBounds(posx, posy)) {
            map.AddHeight(posx, posy, change);
            return change;
        }
        return 0;
    }

}