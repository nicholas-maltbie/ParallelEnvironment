using UnityEngine;

/// <summary>
/// Loads a map from a LargeHeightMap into the world.
/// </summary>
[RequireComponent(typeof(LargeHeightMap))]
public class LargeMapChunkLoader : MonoBehaviour {
    
    /// <summary>
    /// Size of each individual chunk
    /// </summary>
    [Range(16, 256)]
    public int chunkSize = 16;

    /// <summary>
    /// LargeHeight map to load in
    /// </summary>
    private LargeHeightMap heightMap;

    /// <summary>
    /// Shader to apply for once the mesh is generated
    /// </summary>
    public Shader terrainShader;

    /// <summary>
    /// Initializes the chunks and loads height map.
    /// </summary>
    public void Start() {
        heightMap = GetComponent<LargeHeightMap>();
        SetupChunks();
    }

    /// <summary>
    /// Sets up the chunks. Creates a chunk at each offset value.
    /// </summary>
    private void SetupChunks() {
        for (int chunkX = 0; chunkX < Mathf.CeilToInt(heightMap.mapSize / chunkSize); chunkX++) {
            for (int chunkY = 0; chunkY < Mathf.CeilToInt(heightMap.mapSize / chunkSize); chunkY++) {
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
        int offx = chunkX * chunkSize;
        int offy = chunkY * chunkSize;

        // Setup and run the mesh generator for the chunk
        MeshGenerator meshGen = chunk.AddComponent<MeshGenerator>();
        // Make chunk one larger than actual size to include borders between chunks
        meshGen.mapSize = chunkSize + 1;
        meshGen.offsetX = offx;
        meshGen.offsetY = offy;
        meshGen.terrainShader = terrainShader;
        // Generate mesh
        meshGen.SetupMesh(heightMap);
        
        chunk.transform.parent = transform;
        chunk.transform.position = new Vector3(offx, 0, offy);
    }
}