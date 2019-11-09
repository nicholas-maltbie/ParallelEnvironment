using UnityEngine;
using System;

public class HydroErosion : MonoBehaviour {

    private static float sqrt2PI = Mathf.Sqrt(2 * Mathf.PI);

    public int seed = 0;

    /// <summary>
    /// Inertia value for how much momentum is preserved.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float inertia = 0.05f;
    /// <summary>
    /// Speed at which water evaporates from the droplet
    /// </summary>
    [Range (0, 1)]
    public float evaporateSpeed = .01f;

    /// <summary>
    /// Maximum time that a droplet can survive
    /// </summary>
    public int maxDropletLifetime = 30;

    /// <summary>
    /// Initial speed of a droplet
    /// </summary>
    public float initialSpeed = 1.0f;
    /// <summary>
    /// Initial water content of a droplet
    /// </summary>
    public float initialWater = 1;

    /// <summary>
    /// Factor of gravity for moving down sides of mountain
    /// </summary>
    public float gravity = 4.0f;

    /// <summary>
    /// Amount of sediment that a single droplet can carry.
    /// </summary>
    public float sedimentCapacityFactor = 4;
    /// <summary>
    /// Minimum sediment that a droplet can carry.
    /// Used to prevent carry capacity getting too close to zero on flatter terrain
    /// </summary>
    public float minSedimentCapacity = .01f;
    /// <summary>
    /// Speed at which sediment is removed from surronding area
    /// </summary>
    [Range (0, 1)]
    public float erodeSpeed = .3f;
    /// <summary>
    /// Speed at which sediment is added to the surronding area
    /// </summary>
    [Range (0, 1)]
    public float depositSpeed = .3f;

    /// <summary>
    /// Radius for gaussian blur when eroding
    /// </summary>
    [Range(0, 10)]
    public int erodeRadius = 3;

    [Range(0, 1)]
    public float speedFactor = 0;

    private System.Random prng;

    public GameObject debugSphere;

    public void ErodeHeightMap(HeightMap map, int startX, int startY, int endX, int endY, int iterations) {

        if (prng == null) {
            prng = seed == 0 ? new System.Random() : new System.Random(seed);
        }

        int dx = endX - startX;
        int dy = endY - startY;

        Vector2 momentum = new Vector2(0, 0);

        // Iteration for each raindrop
        for (int iter = 0; iter < iterations; iter++) {
            // Simulate each raindrop
            Vector2 position = new Vector2(prng.Next() % dx + startX, prng.Next() % dy + startY);
            float speed = initialSpeed;
            float water = initialWater;
            float sediment = 0;


            for (int lifetime = 0; lifetime < maxDropletLifetime; lifetime++) {
                
                /*
                GameObject debug = Instantiate(debugSphere, new Vector3((int)position.x, map.GetHeight((int)position.x, (int)position.y), (int)position.y), Quaternion.Euler(Vector3.zero));
                debug.transform.localScale = Vector3.one * (sediment / sedimentCapacityFactor + 0.5f);
                */

                // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
                HeightAndGradient heightAndGradient = CalculateHeightAndGradient(map, position);

                // Update the droplet's direction and position (move position 1 unit regardless of speed)
                momentum = (momentum * inertia - heightAndGradient.gradientVec * (1 - inertia)).normalized;
                int nodeX = Mathf.FloorToInt(position.x);
                int nodeY = Mathf.FloorToInt(position.y);

                float cellOffsetX = position.x - nodeX;
                float cellOffsetY = position.y - nodeY;

                position += momentum;

                // Stop simulating droplet if it's not moving or has flowed over edge of map
                if (!map.IsInBounds(nodeX, nodeY)) {
                    break;
                }

                // Find the droplet's new height and calculate the deltaHeight
                float newHeight = CalculateHeightAndGradient (map, position).height;
                float deltaHeight = newHeight - heightAndGradient.height;

                // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
                float sedimentCapacity = Mathf.Max (-deltaHeight * (speed * speedFactor + 1.0f) * water * sedimentCapacityFactor, minSedimentCapacity);


                //Debug.Log("sediment: " + sediment + " capacity: " + sedimentCapacity);

                // If carrying more sediment than capacity, or if flowing uphill:
                if (sediment > sedimentCapacity || deltaHeight > 0) {
                    // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                    float amountToDeposit = (deltaHeight > 0) ? Mathf.Min (deltaHeight, sediment) : (sediment - sedimentCapacity) * depositSpeed;
                    //Debug.Log("Adding " + amountToDeposit + " to height map");
                    // Add the sediment to the four nodes of the current cell using bilinear interpolation
                    // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                    
                    
                    sediment -= ChangeHeightMap(map, nodeX, nodeY, amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY));
                    sediment -= ChangeHeightMap(map, nodeX + 1, nodeY, amountToDeposit * cellOffsetX * (1 - cellOffsetY));
                    sediment -= ChangeHeightMap(map, nodeX, nodeY + 1, amountToDeposit * (1 - cellOffsetX) * cellOffsetY);
                    sediment -= ChangeHeightMap(map, nodeX + 1, nodeY + 1, amountToDeposit * cellOffsetX * cellOffsetY);
                    
                    //sediment -= AddGaussianHeight(map, position - momentum, erodeRadius, amountToDeposit);

                } else {
                    // Erode a fraction of the droplet's current carry capacity.
                    // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                    float amountToErode = Mathf.Min ((sedimentCapacity - sediment) * erodeSpeed, -deltaHeight);

                    //Debug.Log(deltaHeight + ", " + amountToErode);

                    // Remove the sediment to the four nodes of the current cell using bilinear interpolation
                    // sediment -= AddHeightBiLinear(map, pos, -amountToErode);

                    //sediment += GaussianErodeLocation(map, position - momentum, amountToErode, erodeRadius, newHeight);
                    sediment += DistanceErodeLocation(map, nodeX, nodeY, erodeRadius, amountToErode);
                    
                    //sediment += amountToErode;
                    //Debug.Log("target: " + amountToErode + ", before: " + before + ", after: " + sediment);
                }


                // Update droplet's speed and water content
                speed = Mathf.Sqrt (speed * speed + deltaHeight * gravity);
                water *= (1 - evaporateSpeed);
            }

        }
    }

    private float AddGaussianHeight(HeightMap map, Vector2 pos, int radius, float amount) {
        float totalKernel = 0;
        float[,] kernel = new float[radius * 2 + 1,radius * 2 + 1];
        float sd = radius / 3f;
        float offsetX = pos.x % 1.0f;
        float offsetY = pos.y % 1.0f;
        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                if (!map.IsInBounds((int)pos.x + x, (int)pos.y + y) || x * x + y * y > radius * radius) {
                    continue;
                }
                float dx = x + offsetX;
                float dy = y + offsetY;
                kernel[x+radius, y+radius] = 1 / (sqrt2PI * sd * sd) * Mathf.Exp(-(dx * dx + dy * dy) / (2 * sd * sd));
                totalKernel += kernel[x+radius, y+radius];
            }
        }
        
        float totalAdded = 0;

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                if (!map.IsInBounds((int)pos.x + x, (int)pos.y + y) || x * x + y * y > radius * radius) {
                    continue;
                }
                float add = amount * kernel[x + radius, y + radius] / totalKernel;
                map.AddHeight(x + (int)pos.x, y + (int)pos.y, add);
                totalAdded += add;
            }
        }
        return totalAdded;
    }

    private void ApplySmoothGaussKernel(HeightMap map, int posx, int posy, int radius) {
        float totalKernel = 0;
        float[,] kernel = new float[radius * 2 + 1,radius * 2 + 1];
        float sd = radius;
        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                if (!map.IsInBounds(posx + x, posy + y)) {
                    continue;
                }
                kernel[x+radius, y+radius] = 1 / (sqrt2PI * sd * sd) * Mathf.Exp(-(x * x + y * y) / (2 * sd * sd));
                totalKernel += kernel[x+radius, y+radius];
            }
        }

        float wSum = 0;

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                if (!map.IsInBounds(posx + x, posy + y)) {
                    continue;
                }
                wSum += kernel[x + radius, y + radius] / totalKernel * map.GetHeight(x + posx, y + posy);
            }
        }
        map.SetHeight(posx, posy, wSum);
    }

    private float DistanceErodeLocation(HeightMap map, int posx, int posy, int radius, float amount) {
        float[,] weights = new float[radius * 2 + 1, radius * 2 + 1];
        float weightSum = 0;
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                float sqrDst = x * x + y * y;
                if (sqrDst < radius * radius || !map.IsInBounds(posx + x, posy + y)) {
                    continue;
                }
                weights[x + radius, y + radius] = 1 - Mathf.Sqrt (sqrDst) / radius;
                weightSum += weights[x + radius, y + radius];
            }
        }
        float[,] brush = new float[radius * 2 + 1, radius * 2 + 1];
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                brush[x + radius, y + radius] = weights[x + radius, y + radius] / weightSum;
            }
        }

        float totalChange = 0;

        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                float sqrDst = x * x + y * y;
                if (sqrDst < radius * radius || !map.IsInBounds(posx + x, posy + y)) {
                    continue;
                }
                float weightedErodeAmount = amount * brush[x + radius, y + radius];
                map.AddHeight(x + posx, y + posy, -weightedErodeAmount);
                totalChange += weightedErodeAmount;
            }
        }
        return totalChange;
    }

    private float GaussianErodeLocation(HeightMap map, Vector2 pos, float change, int radius, float newHeight) {
        float totalKernel = 0;
        float[,] kernel = new float[radius * 2 + 1,radius * 2 + 1];
        float sd = radius / 3f;
        float offsetX = pos.x % 1.0f;
        float offsetY = pos.y % 1.0f;
        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                float deltaH = map.GetHeight(x + (int)pos.x, y + (int)pos.y) - newHeight;
                if (!map.IsInBounds((int)pos.x + x, (int)pos.y + y) || x * x + y * y > radius * radius || map.GetHeight(x + (int)pos.x, y + (int)pos.y) < newHeight) {
                    continue;
                }
                float dx = x + offsetX;
                float dy = y + offsetY;
                kernel[x+radius, y+radius] = 1 / (sqrt2PI * sd * sd) * Mathf.Exp(-(dx * dx + dy * dy) / (2 * sd * sd));
                totalKernel += kernel[x+radius, y+radius];
            }
        }
        
        float totalEroded = 0;

        for (int x = -radius; x < radius; x++) {
            for (int y = -radius; y < radius; y++) {
                if (!map.IsInBounds((int)pos.x + x, (int)pos.y + y) || x * x + y * y > radius * radius || map.GetHeight(x + (int)pos.x, y + (int)pos.y) < newHeight) {
                    continue;
                }
                float deltaHeight = map.GetHeight(x + (int)pos.x, y + (int)pos.y) - newHeight;
                float erode = Mathf.Min(kernel[x+radius, y+radius] / totalKernel * change, deltaHeight);
                map.AddHeight(x + (int)pos.x, y + (int)pos.y, -erode);
                totalEroded += erode;
            }
        }
        return totalEroded;
    }

    private float ChangeHeightMap(HeightMap map, int posx, int posy, float change) {
        if (map.IsInBounds(posx, posy)) {
            map.AddHeight(posx, posy, change);
            return change;
        }
        return 0;
    }

    private HeightAndGradient CalculateHeightAndGradient (HeightMap map, Vector2 pos) {
        int coordX = (int) pos.x;
        int coordY = (int) pos.y;

        // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        float x = pos.x - coordX;
        float y = pos.y - coordY;

        // Calculate heights of the four nodes of the droplet's cell
        float heightNW = map.GetHeight(coordX, coordY);
        float heightNE = map.GetHeight(coordX + 1, coordY);
        float heightSW = map.GetHeight(coordX, coordY + 1);
        float heightSE = map.GetHeight(coordX + 1, coordY + 1);

        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;

        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        float height = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y + heightSE * x * y;

        return new HeightAndGradient () { height = height, gradientVec = new Vector2(gradientX, gradientY) };
    }

    struct HeightAndGradient {
        public float height;
        public Vector2 gradientVec;
    }

}