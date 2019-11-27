using System;

namespace Terrain.Erosion {
    /// <summary>
    /// Different types of hydro erosion
    /// </summary>
    public enum HydroErosionType {
        Serial,
        StateTransactionalMemory,
        ParallelSpinLocks,
        GPUSpinLocks
    }

    /// <summary>
    /// Functions for mesh generator type
    /// </summary>
    static class HydroErosionTypeMethods {
        /// <summary>
        /// Gets a constructed hydro erosion based on the HydroErosionType.
        /// </summary>
        /// <param name="hydroErosionType">HydroErosionType being returned.</param>
        /// <returns>An instance of the type of erosion being used.</returns>
        public static IHydroErosion ConstructErosion(this HydroErosionType hydroErosionType) {
            switch (hydroErosionType) {
                case HydroErosionType.GPUSpinLocks:
                    return new GPUHydroErosion();
                case HydroErosionType.ParallelSpinLocks:
                    return new PSLHydroErosion();
                case HydroErosionType.StateTransactionalMemory:
                    return new STMHydroErosion();
                case HydroErosionType.Serial:
                default:
                    return new SerialHydroErosion();
            }
        }
    }
}
