
using UnityEngine;
using System.Linq;

namespace Terrain.MeshGen {
    public class ParallelMeshGenerator : AbstractMeshGenerator
    {
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

        public override Vector3[] GetMeshNormals()
        {
            // Vector of normals for each vertex
            Vector3[] normals = new Vector3[GetMapSize() * GetMapSize()];

            ParallelEnumerable.Range(0, GetMapSize() * GetMapSize()).ForAll(
                index => {
                    int x = index % GetMapSize();
                    int y = index / GetMapSize();
                    int mapIndex = GetMapIndex(x, y);
                    
                    // Calculate the normal mapping for that coordinate
                    normals[mapIndex] = CalculateNormal(x, y);
                }
            );

            return normals;
        }

        protected override int[] GetMeshTriangles()
        {
            // Triangles in height map (2x number of squares)
            // One less than map size for border of points
            int[] triangles = new int[(GetMapSize() - 1) * (GetMapSize() - 1) * 6];

            // Create triangles based on mesh
            // Two triangles for each x and y position on height map
            // Need vertices on edge so ignore the far edge
            ParallelEnumerable.Range(0, (GetMapSize() - 1) * (GetMapSize() - 1)).ForAll(
                index => {
                    int x = index % (GetMapSize() - 1);
                    int y = index / (GetMapSize() - 1);
                    // First triangle goes in order (triX, triY), (triX, triY+1), (triX+1, triY)
                    // Second triangle goes in order (triX + 1, triY), (triX, triY+1), (triX+1, triY+1)
                    // Need this order of triangles so mesh renders in correct direction (clockise order of vertices)
                    int tri1Start = (x + y * (GetMapSize() - 1)) * 6;
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
            );

            return triangles;
        }

        public override Vector3[] GetMeshVertices()
        {
            Vector3[] vertices = new Vector3[GetMapSize() * GetMapSize()];
            // Create vertices based on noise map
            ParallelEnumerable.Range(0, GetMapSize() * GetMapSize()).ForAll(
                index => {
                    int x = index % GetMapSize();
                    int y = index / GetMapSize();
                    int mapIndex = GetMapIndex(x, y);
                    // Create a vertex at the specified height
                    vertices[mapIndex] = new Vector3(x, GetLocalHeight(x, y), y);
                }
            );

            return vertices;
        }

        protected override Vector2[] GetUVMapping()
        {
            // Size of each pixel in uv mapping
            float uvStep = 1.0f / GetMapSize();
            // UVMapping for height map (same as number of vertices)
            Vector2[] uvMapping = new Vector2[GetMapSize() * GetMapSize()];

            // Create vertices based on noise map
            ParallelEnumerable.Range(0, GetMapSize() * GetMapSize()).ForAll(
                index => {
                    int x = index % GetMapSize();
                    int y = index / GetMapSize();
                    int mapIndex = GetMapIndex(x, y);                    
                    // Set the UV Coordinates for each vertex
                    uvMapping[mapIndex] = new Vector2(x * uvStep, y * uvStep);
                }
            );

            return uvMapping;
        }
    }
}