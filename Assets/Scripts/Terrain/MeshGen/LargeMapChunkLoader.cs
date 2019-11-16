using UnityEngine;
using Terrain.Map;
using Terrain.Erosion;

namespace Terrain.MeshGen {
    /// <summary>
    /// Loads a map from a LargeHeightMap into the world.
    /// </summary>
    [RequireComponent(typeof(LargeHeightMap))]
    [RequireComponent(typeof(AbstractHydroErosion))]
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
        /// Elapsed time since last ersion.
        /// </summary>
        private float elapsed = 0;
        /// <summary>
        /// Current number of droplets created.
        /// </summary>
        private int progress = 0;

        /// <summary>
        /// Interval between eroding
        /// </summary>
        public float erodeInterval = 0.1f;
        /// <summary>
        /// Number of droplets to create per interval
        /// </summary>
        public int dropletsPerInterval = 100;
        /// <summary>
        /// Total number of droplets to create
        /// </summary>
        public int totalDroplets = 60000;

        /// <summary>
        /// Type of mesh generator being used in this chunk loader.
        /// </summary>
        public MeshGenType meshGenType;

        /// <summary>
        /// Initializes the chunks and loads height map.
        /// </summary>
        public void Start() {
            this.heightMap = GetComponent<LargeHeightMap>();
            this.heightMap.GenerateHeightMap();

            SetupChunks();
        }

        /// <summary>
        /// Update to do every iteration for erosion.
        /// </summary>
        void Update() {
            if (this.progress < this.totalDroplets) {
                this.elapsed += Time.deltaTime;

                if (this.elapsed > this.erodeInterval) {
                    this.elapsed %= this.erodeInterval;

                    AbstractHydroErosion erosion = GetComponent<AbstractHydroErosion>();
                    erosion.ErodeHeightMap(this.heightMap, 
                        new Vector2Int(0, 0), new Vector2Int(this.heightMap.mapSize, this.heightMap.mapSize),
                        this.dropletsPerInterval);
                    this.progress += this.dropletsPerInterval;

                    UpdateMeshes();
                }
            }
        }

        /// <summary>
        /// Updates the mesh for all chunks in the map.
        /// </summary>
        private void UpdateMeshes() {
            foreach (AbstractMeshGenerator gen in gameObject.GetComponentsInChildren<AbstractMeshGenerator>()) {
                gen.UpdateGeometry();
            }
        }

        /// <summary>
        /// Sets up the chunks. Creates a chunk at each offset value.
        /// </summary>
        private void SetupChunks() {
            for (int chunkX = 0; chunkX < Mathf.CeilToInt(this.heightMap.mapSize / this.chunkSize); chunkX++) {
                for (int chunkY = 0; chunkY < Mathf.CeilToInt(this.heightMap.mapSize / this.chunkSize); chunkY++) {
                    LoadChunk(chunkX, chunkY);
                }
            }
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
            meshGen.SetupMesh(this.heightMap, new Vector2Int(offX, offY), this.chunkSize + 1,
                this.terrainShader, this.terrainMaterial);
            
            chunk.transform.parent = transform;
            chunk.transform.position = new Vector3(offX, 0, offY);
        }
    }
}