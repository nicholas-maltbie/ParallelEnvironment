using UnityEngine;
using UnityEditor;
using Terrain.Erosion;

namespace Terrain.MeshGen {
    /// <summary>
    /// Editor menu for generating mesh and eroding the height map.
    /// </summary>
    [CustomEditor(typeof(LargeMapChunkLoader))]
    public class LargeMapChunkLoaderEditor : Editor {
        /// <summary>
        /// Has the mesh been created yet
        /// </summary>
        private bool setup = false;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            // Add button to generate mesh
            if(GUILayout.Button("Generate Mesh")) {
                LargeMapChunkLoader loader = (LargeMapChunkLoader)target;
                loader.SetupChunks();
                setup = true;
            }
            // Run erosion for 50000 iterations
            if(GUILayout.Button("Erode 50K droplets")) {
                if (setup) {
                    LargeMapChunkLoader loader = (LargeMapChunkLoader)target;
                    loader.DoErosion(50000);
                }
                else {
                    Debug.Log("Need to generate mesh before erosion");
                }
            }
        }
    }
}
