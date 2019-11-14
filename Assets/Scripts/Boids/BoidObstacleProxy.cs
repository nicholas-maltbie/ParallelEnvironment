using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    public struct BoidObstacle : IComponentData
    { }
    
    [DisallowMultipleComponent] 
    public class BoidObstacleProxy : ComponentDataProxy<BoidObstacle> { }
}