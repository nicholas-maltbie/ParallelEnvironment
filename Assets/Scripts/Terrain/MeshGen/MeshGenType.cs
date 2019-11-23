using System;

namespace Terrain.MeshGen {
    /// <summary>
    /// Different types of mesh generators
    /// </summary>
    public enum MeshGenType {
        Serial,
        Parallel
    }

    /// <summary>
    /// Functions for mesh generator type
    /// </summary>
    static class MeshGenTypeMethods {
        /// <summary>
        /// Gets the type of component based on the MeshGenType.
        /// </summary>
        /// <param name="meshGenType">MeshGenType being returned.</param>
        /// <returns>Some type that extends AbstractMeshGenerator</returns>
        public static System.Type GetMeshGenType(this MeshGenType meshGenType) {
            switch (meshGenType) {
                case MeshGenType.Parallel:
                    return typeof(ParallelMeshGenerator);
                case MeshGenType.Serial:
                default:
                    return typeof(SerialMeshGenerator);
            }
        }
    }
}