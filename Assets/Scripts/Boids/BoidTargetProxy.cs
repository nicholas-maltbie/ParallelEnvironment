using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    public struct BoidTarget : IComponentData
    { }
    
    [DisallowMultipleComponent] 
    public class BoidTargetProxy : ComponentDataProxy<BoidTarget> { }
}