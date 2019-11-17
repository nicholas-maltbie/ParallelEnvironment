using System;

namespace Terrain.Erosion {
    /// <summary>
    /// Different types of hydro erosion
    /// </summary>
    public enum HydroErosionType {
        Serial,
    }

    /// <summary>
    /// Functions for mesh generator type
    /// </summary>
    static class HydroErosionTypeMethods {
        /// <summary>
        /// Gets the type of component based on the HydroErosionType.
        /// </summary>
        /// <param name="meshGenType">HydroErosionType being returned.</param>
        /// <returns>Some type that extends AbstractMeshGenerator</returns>
        public static System.Type GetMeshGenType(this HydroErosionType meshGenType) {
            switch (meshGenType) {
                case HydroErosionType.Serial:
                default:
                    return typeof(SerialHydroErosion);
            }
        }
    }
}
