using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a mesh for an object using its HeightMapGenerator Component.
/// </summary>
[RequireComponent(typeof(AbstractHeightMapGenerator))]
public class MeshGenerator : MonoBehaviour
{
    /// <summary>
    /// Size of the map square. This is the number of vertices on an edge of the map.
    /// The total number of vertices is mapSize * mapSize. The number of squares will
    /// be (mapSize -1) * (mapSize -1) because a line of vertices is needed for the edge.
    /// </summary>
    [Range(2, 2048)]
    public int mapSize = 64;

    /// <summary>
    /// Minimum height of the map, values in the height map that are zero will 
    /// be at this height.
    /// </summary>
    public int minHeight = 0;
    /// <summary>
    /// Maximum height of the map, values in the height map that are one will
    /// be at this height.
    /// </summary>
    public int maxHeight = 256;
    /// <summary>
    /// Range from the lowest pixel to the highest pixel. Calculated as maxHeight - minHeight.
    /// </summary>
    private int heightRange;

    /// <summary>
    /// Shader to apply for once the mesh is generated
    /// </summary>
    public Shader terrainShader;

    /// <summary>
    /// Saved values of the height map
    /// </summary>
    private float[] heightMap;

    /// <summary>
    /// Generator attached to this object to create a height map.
    /// </summary>
    private AbstractHeightMapGenerator mapGenerator;

    /// <summary>
    /// Container object for holding this mesh. Should be a child object
    /// to this class.
    /// </summary>
    private GameObject meshContainer;
    /// <summary>
    /// Renderer associated with the mesh container.
    /// </summary>
    private MeshRenderer meshRenderer;
    /// <summary>
    /// Mesh filter associated with the mesh container
    /// </summary>
    private MeshFilter meshFilter;
    /// <summary>
    /// the mesh used in the mesh filter
    /// </summary>
    private Mesh heightMapMesh;

    /// <summary>
    /// Initialize this mesh and height map.
    /// </summary>
    public void Start() {
        // Get the map generator component from this object
        mapGenerator = GetComponent<AbstractHeightMapGenerator>();
        // Setup the height map for the mesh
        SetupHeightMap();
        // Create the mesh based on the height map
        SetupMesh();
    }

    /// <summary>
    /// Initializes the height map variable for this instance based on mapGenerator
    /// </summary>
    private void SetupHeightMap() {
        // Find the range of heights from specified fields
        heightRange = maxHeight - minHeight;
        // Generate the height map from the mesh
        heightMap = mapGenerator.CreateHeightMap(mapSize);
        // Scale height map to be between minHeight and maxHeight
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int mapIndex = y * mapSize + x;
                heightMap[mapIndex] = heightMap[mapIndex] * heightRange + minHeight;
            }
        }
    }

    /// <summary>
    /// Creates the mesh initially from the specified height map and settings.
    /// </summary>
    private void SetupMesh() {
        // Create the mesh container and add required components
        meshContainer = new GameObject();
        meshContainer.name = "MeshContainer";
        meshContainer.transform.parent = gameObject.transform;
        meshRenderer = meshContainer.AddComponent<MeshRenderer>();
        meshFilter = meshContainer.AddComponent<MeshFilter>();

        // Generate height map based on these values
        CreateMeshFromHeightMap();

        Material terrainMaterial = new Material(terrainShader);
        meshRenderer.material = terrainMaterial;
    }

    /// <summary>
    /// Gets the height at a specified location in the grid. If the 
    /// location is out of bounds of the map, it will bound it 
    /// to the closest location.
    /// </summary>
    /// <param name="x">X location in grid (column)</param>
    /// <param name="y">Y location in grid (row)</param>
    /// <returns>
    /// Height at specified location in grid
    /// (bounded between min and max heights by noise value)
    /// </returns>
    private float GetHeight(int x, int y) {
        // Bound x and y by map size
        x = Mathf.Min(Mathf.Max(0, x), mapSize - 1);
        y = Mathf.Min(Mathf.Max(0, y), mapSize - 1);
        // Return the closest value at the height map for that given location
        return heightMap[GetMapIndex(x, y)];
    }

    /// <summary>
    /// Calculates the normal of the terrain at a given X and Y coordinate
    /// </summary>
    /// <param name="x">X coordinate at the grid</param>
    /// <param name="y">Y coordinate at the grid</param>
    /// <returns>Returns the vector normal to that location on the grid</returns>
    private Vector3 CalculateNormal(int x, int y) {
        // Read the nearest neighbor's heights
        int offset = 1;
        float hL = GetHeight(x - offset, y);
        float hR = GetHeight(x + offset, y);
        float hD = GetHeight(x, y - offset);
        float hU = GetHeight(x, y + offset);
        // compute terrain normal
        Vector3 norm = new Vector3(hL - hR, 2.0f, hD - hU);
        return norm.normalized;
    }

    private int GetMapIndex(int x, int y) {
        return x + y * mapSize;
    }

    /// <summary>
    /// Creates a height map mesh from the given height map generator field.
    /// Creates a simple grid mesh then connects each square with two triangles. 
    /// </summary>
    private void CreateMeshFromHeightMap() {
        // Size of each pixel in uv mapping
        float uvStep = 1.0f / mapSize;

        // Map of vectors for vertices in mesh
        Vector3[] vertices = new Vector3[mapSize * mapSize];
        // Vector of normals for each vertex
        Vector3[] normals = new Vector3[mapSize * mapSize];
        // UVMapping for height map (same as number of vertices)
        Vector2[] uvMapping = new Vector2[mapSize * mapSize];
        // Triangles in height map (2x number of squares)
        // One less than map size for border of points
        int[] triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];

        // Create vertices based on noise map
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int mapIndex = GetMapIndex(x, y);
                // Create a vertex at the specified height
                vertices[mapIndex] = new Vector3(x, GetHeight(x, y), y);
            }
        }

        // Create UV mapping for the coordinates
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int mapIndex = GetMapIndex(x, y);
                // Set the UV Coordinates for each vertex
                uvMapping[mapIndex] = new Vector2(x * uvStep, y * uvStep);
            }
        }
        
        // Calculate normals at each point on the map
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                int mapIndex = GetMapIndex(x, y);
                // Calculate the normal mapping for that coordinate
                normals[mapIndex] = CalculateNormal(x, y);
            }
        }

        int tri = 0;
        // Create triangles based on mesh
        // Two triangles for each x and y position on height map
        // Need vertices on edge so ignore the far edge
        for (int x = 0; x < mapSize - 1; x++) {
            for (int y = 0; y < mapSize - 1; y++) {
                if (x == mapSize - 1 || y == mapSize - 1) {
                    continue;
                }
                // First triangle goes in order (trix, triy), (trix, triy+1), (trix+1, triy)
                // Second triangle goes in order (trix + 1, triy), (trix, triy+1), (trix+1, triy+1)
                // Need this order of triangles so mesh renders in correct direction (clockise order of vertices)
                int tri1Start = tri;
                int tri2Start = tri1Start + 3;

                // index of vertex at position (trix, triy)
                triangles[tri1Start] = GetMapIndex(x, y);
                // index of vertex at position (trix, triy + 1)
                triangles[tri1Start + 1] = GetMapIndex(x, y + 1);
                // index of vertex at position (trix + 1, triy)
                triangles[tri1Start + 2] = GetMapIndex(x + 1, y);

                triangles[tri2Start] = GetMapIndex(x + 1, y);
                triangles[tri2Start + 1] = GetMapIndex(x, y + 1);
                triangles[tri2Start + 2] = GetMapIndex(x + 1, y + 1);

                tri += 6;
            }
        }

        // Create mesh from given values
        heightMapMesh = new Mesh();
        heightMapMesh.vertices = vertices;
        heightMapMesh.triangles = triangles;
        heightMapMesh.uv = uvMapping;
        heightMapMesh.normals = normals;
        
        Debug.Log(heightMapMesh.bounds);

        meshFilter.mesh = heightMapMesh;
    }
}
