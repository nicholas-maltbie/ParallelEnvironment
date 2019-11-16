using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain.MeshGen {
    /// <summary>
    /// Generates a mesh for an object using it a HeightMap object.
    /// </summary>
    public class MeshGenerator : MonoBehaviour
    {
        /// <summary>
        /// Size of the map square. This is the number of vertices on an edge of the map.
        /// The total number of vertices is mapSize * mapSize. The number of squares will
        /// be (mapSize -1) * (mapSize -1) because a line of vertices is needed for the edge.
        /// </summary>
        [Range(2, 256)]
        public int mapSize = 16;

        /// <summary>
        /// Shader to apply for once the mesh is generated.
        /// AS OF RIGHT NOW THIS IS NOT USED
        /// </summary>
        public Shader terrainShader;
        /// <summary>
        /// Material to apply for once the mesh is generated
        /// </summary>
        public Material terrainMaterial;

        /// <summary>
        /// Offset of this component in x axis for reading values from height map.
        /// </summary>
        public int offsetX = 0;

        /// <summary>
        /// Offset of this component in y axis for reading values from height map.
        /// </summary>
        public int offsetY = 0;

        /// <summary>
        /// Height map component.
        /// </summary>
        public HeightMapComponent heightMap;

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
        /// Creates mesh from specified height map.
        /// </summary>
        /// <param name="heightMap">Height map to use when creating the mesh.</param>
        public void SetupMesh(HeightMapComponent heightMap) {
            this.heightMap = heightMap;

            // Create the mesh container and add required components
            this.meshContainer = new GameObject();
            this.meshContainer.name = "MeshContainer";
            this.meshContainer.transform.parent = gameObject.transform;
            this.meshRenderer = meshContainer.AddComponent<MeshRenderer>();
            this.meshFilter = meshContainer.AddComponent<MeshFilter>();

            // Generate height map based on these values
            CreateMeshFromHeightMap();

            //Material terrainMaterial = new Material(terrainShader);
            this.meshRenderer.material = terrainMaterial;
        }

        /// <summary>
        /// Gets the height at a specified location in the grid. Will translate coordinates from local 
        /// space to world space.
        /// </summary>
        /// <param name="localX">local X location in grid (column)</param>
        /// <param name="localY">local Y location in grid (row)</param>
        /// <returns>
        /// Height at specified location in grid.
        /// </returns>
        private float GetLocalHeight(int localX, int localY) {
            // Return the value at the height map for that given location
            return heightMap.GetHeight(localX + this.offsetX, localY + this.offsetY);
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
            float hL = GetLocalHeight(x - offset, y);
            float hR = GetLocalHeight(x + offset, y);
            float hD = GetLocalHeight(x, y - offset);
            float hU = GetLocalHeight(x, y + offset);
            // compute terrain normal
            Vector3 norm = new Vector3(hL - hR, 2.0f, hD - hU);
            return norm.normalized;
        }

        /// <summary>
        /// Gets the local index of a x and y position in the grid.
        /// </summary>
        /// <param name="x">Local X coordinate in grid</param>
        /// <param name="y">Local Y coordinate in grid</param>
        /// <returns>Index in mesh vertices for a given x and y position</returns>
        private int GetMapIndex(int x, int y) {
            return x + y * this.mapSize;
        }

        /// <summary>
        /// Updates the geometry for this mesh. Recalculates the vertex positions and normals.
        /// </summary>
        public void UpdateGeometry() {
            // Map of vectors for vertices in mesh
            Vector3[] vertices = this.heightMapMesh.vertices;
            // Vector of normals for each vertex
            Vector3[] normals = this.heightMapMesh.normals;
            
            // Create vertices based on noise map
            for (int x = 0; x < this.mapSize; x++) {
                for (int y = 0; y < this.mapSize; y++) {
                    int mapIndex = GetMapIndex(x, y);
                    // Create a vertex at the specified height
                    vertices[mapIndex] = new Vector3(x, GetLocalHeight(x, y), y);
                }
            }
            
            // Calculate normals at each point on the map
            for (int x = 0; x < this.mapSize; x++) {
                for (int y = 0; y < this.mapSize; y++) {
                    int mapIndex = GetMapIndex(x, y);
                    // Calculate the normal mapping for that coordinate
                    normals[mapIndex] = CalculateNormal(x, y);
                }
            }

            this.heightMapMesh.vertices = vertices;
            this.heightMapMesh.normals = normals;
        }

        /// <summary>
        /// Creates a height map mesh from the given height map generator field.
        /// Creates a simple grid mesh then connects each square with two triangles. 
        /// </summary>
        private void CreateMeshFromHeightMap() {
            // Size of each pixel in uv mapping
            float uvStep = 1.0f / this.mapSize;

            // Map of vectors for vertices in mesh
            Vector3[] vertices = new Vector3[this.mapSize * this.mapSize];
            // Vector of normals for each vertex
            Vector3[] normals = new Vector3[this.mapSize * this.mapSize];
            // UVMapping for height map (same as number of vertices)
            Vector2[] uvMapping = new Vector2[this.mapSize * this.mapSize];
            // Triangles in height map (2x number of squares)
            // One less than map size for border of points
            int[] triangles = new int[(this.mapSize - 1) * (this.mapSize - 1) * 6];

            // Create vertices based on noise map
            for (int x = 0; x < this.mapSize; x++) {
                for (int y = 0; y < this.mapSize; y++) {
                    int mapIndex = GetMapIndex(x, y);
                    // Create a vertex at the specified height
                    vertices[mapIndex] = new Vector3(x, GetLocalHeight(x, y), y);
                    
                    // Calculate the normal mapping for that coordinate
                    normals[mapIndex] = CalculateNormal(x, y);
                    
                    // Set the UV Coordinates for each vertex
                    uvMapping[mapIndex] = new Vector2(x * uvStep, y * uvStep);
                }
            }

            // Create triangles based on mesh
            // Two triangles for each x and y position on height map
            // Need vertices on edge so ignore the far edge
            for (int x = 0; x < this.mapSize - 1; x++) {
                for (int y = 0; y < this.mapSize - 1; y++) {
                    // First triangle goes in order (triX, triY), (triX, triY+1), (triX+1, triY)
                    // Second triangle goes in order (triX + 1, triY), (triX, triY+1), (triX+1, triY+1)
                    // Need this order of triangles so mesh renders in correct direction (clockise order of vertices)
                    int tri1Start = (x + y * (this.mapSize - 1)) * 6;
                    int tri2Start = tri1Start + 3;

                    // index of vertex at position (triX, triY)
                    triangles[tri1Start] = GetMapIndex(x, y);
                    // index of vertex at position (triX, triY + 1)
                    triangles[tri1Start + 1] = GetMapIndex(x, y + 1);
                    // index of vertex at position (triX + 1, triY)
                    triangles[tri1Start + 2] = GetMapIndex(x + 1, y);

                    triangles[tri2Start] = GetMapIndex(x + 1, y);
                    triangles[tri2Start + 1] = GetMapIndex(x, y + 1);
                    triangles[tri2Start + 2] = GetMapIndex(x + 1, y + 1);
                }
            }

            // Create mesh from given values
            this.heightMapMesh = new Mesh();
            this.heightMapMesh.vertices = vertices;
            this.heightMapMesh.triangles = triangles;
            this.heightMapMesh.uv = uvMapping;
            this.heightMapMesh.normals = normals;
            
            this.meshFilter.mesh = this.heightMapMesh;
        }
    }
}
