using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a mesh for an object using its HeightMapGenerator Component.
/// </summary>
[RequireComponent(typeof(AbstractHeightMapGenerator))]
public class MeshCreator : MonoBehaviour
{
    [Range(2, 255)]
    public int mapSize = 64;

    public int minHeight = 0;
    public int maxHeight = 255;

    public Shader terrainShader;

    private float[] heightMap;

    private AbstractHeightMapGenerator mapGenerator;

    private GameObject meshContainer;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh heightMapMesh;

    public void Start() {
        mapGenerator = GetComponent<AbstractHeightMapGenerator>();
        SetupMesh();
    }

    private void SetupMesh() {
        // Create the mesh container and add required components
        meshContainer = new GameObject();
        meshContainer.transform.parent = gameObject.transform;
        meshRenderer = meshContainer.AddComponent<MeshRenderer>();
        meshFilter = meshContainer.AddComponent<MeshFilter>();

        // Generate height map based on these values
        CreateMeshFromHeightMap();

        meshRenderer.material = new Material(terrainShader);
    }

    /// <summary>
    /// Creates a height map mesh from the given height map generator field.
    /// Creates a simple grid mesh then connects each square with two triangles. 
    /// </summary>
    private void CreateMeshFromHeightMap() {
        // Generate the height map from the mesh
        heightMap = mapGenerator.CreateHeightMap(mapSize);
        // Find the range of heights from specified fields
        int heightRange = maxHeight - minHeight;

        // Map of vectors for vertices in mesh
        Vector3[] vertices = new Vector3[mapSize * mapSize];
        // Triangles in height map (2x number of squares)
        // One less than map size for border of points
        int[] triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];
        // UVMapping for height map (same as number of triangles)
        Vector2[] uvMapping = new Vector2[(mapSize - 1) * (mapSize - 1) * 6];

        // Create vertices based on noise map
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                // Create a vertex at the specified height
                int mapIndex = y * mapSize + x;
                // Find height based on height map and height range
                float height = heightMap[mapIndex] * heightRange + minHeight;
                // Create vector with this information
                vertices[mapIndex] = new Vector3(x, height, y);
            }
        }

        // Create triangles based on mesh
        // Two triangles for each x and y position on height map
        // Need vertices on edge so ignore the far edge
        for (int trix = 0; trix < mapSize - 1; trix++) {
            for (int triy = 0; triy < mapSize - 1; triy++) {
                // First triangle goes in order (trix, triy), (trix, triy+1), (trix+1, triy)
                // Second triangle goes in order (trix + 1, triy), (trix, triy+1), (trix+1, triy+1)
                // Need this order of triangles so mesh renders in correct direction (clockise order of vertices)
                int tri1Start = (triy * (mapSize - 1) + trix) * 6;
                int tri2Start = (triy * (mapSize - 1) + trix) * 6 + 3;

                // index of vertex at position (trix, triy)
                triangles[tri1Start] = trix + triy * mapSize;
                // index of vertex at position (trix, triy + 1)
                triangles[tri1Start + 1] = trix + (triy + 1) * mapSize;
                // index of vertex at position (trix + 1, triy)
                triangles[tri1Start + 2] = (trix + 1) + triy * mapSize;

                triangles[tri2Start] = (trix + 1) + triy * mapSize;
                triangles[tri2Start + 1] = trix + (triy + 1) * mapSize;
                triangles[tri2Start + 2] = (trix + 1) + (triy + 1) * mapSize;
            }
        }

        // Create mesh from given values
        heightMapMesh = new Mesh();
        heightMapMesh.vertices = vertices;
        heightMapMesh.triangles = triangles;
        heightMapMesh.RecalculateNormals();

        meshFilter.mesh = heightMapMesh;
    }
}
