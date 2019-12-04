using UnityEngine;
using Terrain.Map;
using Terrain.Erosion;
using System;

namespace Terrain.MeshGen {
    /// <summary>
    /// Loads a map from a LargeHeightMap into the world.
    /// </summary>
    [RequireComponent(typeof(LargeHeightMap))]
    [RequireComponent(typeof(HydroErosionOperator))]
    public class LargeMapChunkLoader : MonoBehaviour {
        
        /// <summary>
        /// Size of each individual chunk
        /// </summary>
        [Range(16, 255)]
        public int chunkSize = 16;

        /// <summary>
        /// LargeHeight map to load in
        /// </summary>
        private LargeHeightMap heightMap;

        /// <summary>
        /// Shader to apply for once the mesh is generated
        /// AS OF RIGHT NOT THIS IS NOT USED
        /// </summary>
        public Shader terrainShader;

        /// <summary>
        /// Material to apply to each chunk of terrain when chunks of terrain are generated.
        /// </summary>
        public Material terrainMaterial;

        /// <summary>
        /// Mesh generation parameters as a struct
        /// </summary>
        private MeshGenParams meshGenParams;

        /// <summary>
        /// Type of mesh generator being used in this chunk loader.
        /// </summary>
        public MeshGenType meshGenType;

        /// <summary>
        /// Should performance be debugged
        /// </summary>
        public bool debugPerformance;

        /// <summary>
        /// Initialize the components for this height map.
        /// </summary>
        private void Initialize() {
            this.heightMap = GetComponent<LargeHeightMap>();
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            this.heightMap.GenerateHeightMap();

            if (this.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Time to generate Height Map: " + deltaMillis + " ms");
            }
            this.meshGenParams = new MeshGenParams(
                this.chunkSize,
                this.terrainShader,
                this.terrainMaterial
            );
        }

        /// <summary>
        /// Updates the mesh for all chunks in the map.
        /// </summary>
        private void UpdateMeshes() {
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            foreach (AbstractMeshGenerator gen in gameObject.GetComponentsInChildren<AbstractMeshGenerator>()) {
                gen.UpdateGeometry();
            }

            if (this.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Time to Update Chunks: " + deltaMillis + " ms");
            }
        }

        /// <summary>
        /// Sets up the chunks. Creates a chunk at each offset value.
        /// </summary>
        public void SetupChunks() {
            Initialize();

            foreach (AbstractMeshGenerator child in GetComponentsInChildren<AbstractMeshGenerator>()) {
                DestroyImmediate(child.gameObject);
            }
            int sizeX = Mathf.CeilToInt(this.heightMap.mapSize / this.chunkSize);
            int sizeY = Mathf.CeilToInt(this.heightMap.mapSize / this.chunkSize);
            long startMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            for (int chunkX = 0; chunkX < sizeX; chunkX++) {
                for (int chunkY = 0; chunkY < sizeY; chunkY++) {
                    LoadChunk(chunkX, chunkY);
                }
            }

            if (this.debugPerformance) {
                float deltaMillis = System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startMillis;
                Debug.Log("Time to Load Chunks: " + deltaMillis + " ms");
            }
        }

        /// <summary>
        /// Does a specificed number of erosion iterations
        /// </summary>
        /// <param name="iterations">Number of droplets</param>
        public void DoErosion(int iterations) {
            HydroErosionOperator erosion = GetComponent<HydroErosionOperator>();
            erosion.ErodeHeightMap(this.heightMap, 
                new Vector2Int(0, 0), new Vector2Int(
                    this.heightMap.mapSize, this.heightMap.mapSize), iterations);

            UpdateMeshes();
        }

        /// <summary>
        /// Creates a chunk at a given offset x and offset y.
        /// </summary>
        /// <param name="chunkX">Chunk index in X plane</param>
        /// <param name="chunkY">Chunk index in Y plane</param>
        private void LoadChunk(int chunkX, int chunkY) {
            // Create the chunk and name it
            GameObject chunk = new GameObject();
            chunk.name = "Chunk-" + chunkX + "," + chunkY;

            // Calculate the actual offset in grid spaces
            int offX = chunkX * this.chunkSize;
            int offY = chunkY * this.chunkSize;

            // Setup and run the mesh generator for the chunk
            AbstractMeshGenerator meshGen = chunk.AddComponent(meshGenType.GetMeshGenType()) as AbstractMeshGenerator;
            // Make chunk one larger than actual size to include borders between chunks
            // Generate mesh
            meshGen.SetupMesh(this.heightMap, new Vector2Int(offX, offY),
                this.chunkSize + 1, this.meshGenParams);
            
            chunk.transform.parent = transform;
            chunk.transform.position = new Vector3(offX, 0, offY);
        }
    }
}