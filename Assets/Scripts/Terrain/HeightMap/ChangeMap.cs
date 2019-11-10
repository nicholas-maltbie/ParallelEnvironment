
/// <summary>
/// A height map that is meant to hold changes. Starts will all avlues
/// initialized to zero and can be applied to other height maps.
/// </summary>
public class ChangeMap : HeightMap {
    /// <summary>
    /// Data stored in change map
    /// </summary>
    private float[] map;
    /// <summary>
    /// Dimensions of the map in the x and y axis
    /// </summary>
    private int sizeX, sizeY;

    /// <summary>
    /// Creates a change map with a given set of dimensions.
    /// </summary>
    /// <param name="sizeX">Size of the map along the X axis</param>
    /// <param name="sizeY">Size of teh map along the Y axis</param>
    public ChangeMap(int sizeX, int sizeY) {
        map = new float[sizeX * sizeY];
        this.sizeX = sizeX;
        this.sizeY = sizeY;
    }
    
    /// <summary>
    /// Gets the height of the map at a given x, y coordinate.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <returns>Height at specified position as a float.</returns>
    private int GetIndex(int x, int y) {
        return x + y * sizeX;
    }

    /// <summary>
    /// Adds to a map at the given coordinate by value of change
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <param name="change">Height to add at position x and y.</param>
    public void AddHeight(int x, int y, float change)
    {
        map[GetIndex(x, y)] += change;
    }

    public float GetHeight(int x, int y)
    {
        return map[GetIndex(x, y)];
    }

    /// <summary>
    /// Checks if a coordinate is in the bounds of the heightmap.
    /// </summary>
    /// <param name="x">X position in the height map</param>
    /// <param name="y">Y position in the height map</param>
    /// <returns>True if the specified coordinate is in the bounds of the height map, false otherwise.</returns>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < sizeX && y >= 0 && y < sizeY;
    }

    /// <summary>
    /// Sets the height value at a specified x, y coordinate.
    /// </summary>
    /// <param name="x">X position in grid</param>
    /// <param name="y">Y position in grid</param>
    /// <param name="height">Height to set at position x and y.</param>
    public void SetHeight(int x, int y, float height)
    {
        map[GetIndex(x, y)] = height;
    }

    /// <summary>
    /// Adds all the changes stored in this map to another map (does a sum on the other map)
    /// </summary>
    /// <param name="targetMap"> Map to add changes to. </param>
    public void ApplyChangesToMap(HeightMap targetMap) {
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                targetMap.AddHeight(x, y, GetHeight(x, y));
            }
        }
    }
}