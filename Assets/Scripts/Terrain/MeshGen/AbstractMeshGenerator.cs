using Terrain.Map;
using UnityEngine;

namespace Terrain.MeshGen {
    /// <summary>
    /// Generates a mesh for a section of a height map.
    /// </summary>
    abstract public class AbstractMeshGenerator : MonoBehaviour {
    
        /// <summary>
        /// Size of the map square. This is the number of vertices on an edge of the map.
        /// The total number of vertices is mapSize * mapSize. The number of squares will
        /// be (mapSize -1) * (mapSize -1) because a line of vertices is needed for the edge.
        /// Must be between [1, 256]
        /// </summary>
        private int mapSize;

        /// <summary>
        /// Shader to apply for once the mesh is generated.
        /// AS OF RIGHT NOW THIS IS NOT USED
        /// </summary>
        private Shader terrainShader;
        /// <summary>
        /// Material to apply for once the mesh is generated
        /// </summary>
        private Material terrainMaterial;

        /// <summary>
        /// Offset of this component on the (x,y) axis for reading values from height map.
        /// </summary>
        private Vector2Int offset;
        
        /// <summary>
        /// Height map component.
        /// </summary>
        private IHeightMap heightMap;

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
        /// </summary>
        
        /// <summary>
        /// Creates mesh from specified height map.
        /// </summary>
        /// <param name="heightMap">Height map to use when creating the mesh.</param>
        /// <param name="offset">Offset of this mesh from the global origin in the heightMap component</param>
        /// <param name="mapSize">Size of the map (must be in range [1,256])</param>
        /// <param name="terrainShader">Shader used to render the heightmap</param>
        /// <param name="terrainMaterial">Material used to apply to the height map</param>
        public void SetupMesh(IHeightMap heightMap, Vector2Int offset, int mapSize,
            Shader terrainShader, Material terrainMaterial) {
            this.heightMap = heightMap;
            this.offset = offset;
            this.mapSize = mapSize;
            this.terrainShader = terrainShader;
            this.terrainMaterial = terrainMaterial;

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
        /// Gets the local index of a x and y position in the grid.
        /// </summary>
        /// <param name="x">Local X coordinate in grid</param>
        /// <param name="y">Local Y coordinate in grid</param>
        /// <returns>Index in mesh vertices for a given x and y position</returns>
        protected int GetMapIndex(int x, int y) {
            return x + y * this.mapSize;
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
        protected float GetLocalHeight(int localX, int localY) {
            // Return the value at the height map for that given location
            return heightMap.GetHeight(localX + this.offset.x, localY + this.offset.y);
        }

        /// <summary>
        /// Updates the geometry for this mesh. Recalculates the vertex positions and normals.
        /// </summary>
        public void UpdateGeometry() {
            this.heightMapMesh.vertices = GetMeshVertices();
            this.heightMapMesh.normals = GetMeshNormals();
        }

        
        /// <summary>
        /// Creates a height map mesh from the given height map generator field.
        /// Creates a simple grid mesh then connects each square with two triangles. 
        /// </summary>
        private void CreateMeshFromHeightMap() {
            // Create mesh from given values
            this.heightMapMesh = new Mesh();
            this.heightMapMesh.vertices = GetMeshVertices();
            this.heightMapMesh.triangles = GetMeshTriangles();
            this.heightMapMesh.uv = GetUVMapping();
            this.heightMapMesh.normals = GetMeshNormals();
            
            this.meshFilter.mesh = this.heightMapMesh;
        }

        /// <summary>
        /// Get the size of the map (square map)
        /// </summary>
        /// <returns>An integer between [1, 256] of the size of the map.</returns>
        protected int GetMapSize() {
            return this.mapSize;
        }

        /// <summary>
        /// Get the UV mapping for each coordinate in the map.
        /// </summary>
        /// <returns>This should be a 2d vector array of size mapSize * mapSize. Each
        /// vector should be a value between [(0.0, 0.0), (1.0, 1.0)]. This
        /// represents the mapping of each vertex to a location on a texture map.</returns>
        abstract protected Vector2[] GetUVMapping();

        /// <summary>
        /// This will generate the vertices. This should be a mapping of each vertex
        /// to a 3d coordinate following the height map of this class.
        /// </summary>
        /// <returns>A vector3 array of mapSize * mapSize of the coordinates of each mesh.</returns>
        abstract protected Vector3[] GetMeshVertices();

        /// <summary>
        /// Get the normal mapping for each vertex. This determines how light interacts
        /// with the mesh and how light should bounce. There should be one normal
        /// vector for each vertex in the mesh.
        /// </summary>
        /// <returns>A vector3 array of mapSize * mapSize that has the normal
        /// direction to the surface at each vertex in the mesh.</returns>
        abstract protected Vector3[] GetMeshNormals();

        /// <summary>
        /// This should return the triangles. There should be 3x the number
        /// of triangles in size. It needs to specify triangle vertex indices
        /// in the order of (t1v1, t1v2, t1v3, t2v1, t2v2, t3v3, ..., tnv1, tnv2, tnv3).
        /// The order of vertices in the triangles determines whether the face is
        /// pointing forward (clockwise) or backward counterclockwise.
        /// When making a square mesh, the number of triangles should be
        /// (mapSize - 1) * (mapSize - 1) * 2, so the size of this array 
        /// should be numTriangles * 3.
        /// </summary>
        /// <returns>A mapping of triangles where every three vertices
        /// corresponds to a triangle in the mesh.</returns>
        abstract protected int[] GetMeshTriangles();
    }
}