
using UnityEngine;

namespace Terrain.MeshGen {
    public struct MeshGenParams {

        /// <summary>
        /// Creates a set of mesh generation parameters
        /// </summary>
        /// <param name="chunkSize">Size of each chunk</param>
        /// <param name="terrainShader">Shader to color terrain</param>
        /// <param name="terrainMaterial">Material to apply to each chunk of terrain when chunks of terrain are generated</param>
        public MeshGenParams(int chunkSize, Shader terrainShader, Material terrainMaterial) {
            this.chunkSize = chunkSize;
            this.terrainShader = terrainShader;
            this.terrainMaterial = terrainMaterial;
        }

        /// <summary>
        /// Material to apply to each chunk of terrain when chunks of terrain are generated.
        /// </summary>
        public Material terrainMaterial;
        /// <summary>
        /// Shader to apply for once the mesh is generated
        /// AS OF RIGHT NOT THIS IS NOT USED
        /// </summary>
        public Shader terrainShader;
        /// <summary>
        /// Size of each chunk in the generated mesh
        /// </summary>
        public readonly int chunkSize;
    }
}
