using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    /// <summary>
    /// A Boid Obstacle component is used in the BoidSystem to mark an Entity
    /// as an obstacle and to be avoided.
    /// </summary>
    public struct BoidObstacle : IComponentData
    { }
    
    [DisallowMultipleComponent] 
    public class BoidObstacleProxy : ComponentDataProxy<BoidObstacle> { }
}