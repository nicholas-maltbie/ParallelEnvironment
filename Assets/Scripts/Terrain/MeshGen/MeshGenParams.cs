
using UnityEngine;

namespace Terrain.MeshGen {
    public struct MeshGenParams {

        /// <summary>
        /// Creates a set of mesh generation parameters
        /// </summary>
        /// <param name="chunkSize">Size of each chunk</param>
        /// <param name="meshGenGPU">GPU component to generate parts of mesh on GPU</param>
        /// <param name="terrainShader">Shader to color terrain</param>
        /// <param name="terrainMaterial">Material to apply to each chunk of terrain when chunks of terrain are generated</param>
        public MeshGenParams(int chunkSize, ComputeShader meshGenGPU, Shader terrainShader, Material terrainMaterial) {
            this.chunkSize = chunkSize;
            this.terrainShader = terrainShader;
            this.meshGenGPU = meshGenGPU;
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
        /// <summary>
        /// GPU Compute shader to generate parts of mesh
        /// </summary>
        public readonly ComputeShader meshGenGPU;
    }
}
