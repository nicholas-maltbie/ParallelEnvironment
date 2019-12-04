using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    ///<summary>
    /// Component to hold data necessary to spawn the given prefab in a sphere.
    ///This Component can be attached to an empty game object along with the convert to entity script. 
    ///</summary>
    [Serializable]
    public struct SpawnRandomInSphere : ISharedComponentData
    {
        public Entity Prefab;
        public float Radius;
        public int Count;
    }

    namespace Authoring
    {
        [RequiresEntityConversion]
        public class SpawnRandomInSphere : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
        {
            public GameObject Prefab;
            public float Radius;
            public int Count;
    
            // Lets you convert the editor data representation to the entity optimal runtime representation
            public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                var spawnerData = new Boids.SpawnRandomInSphere
                {
                    // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                    // So here we simply map the game object to an entity reference to that prefab.
                    Prefab = conversionSystem.GetPrimaryEntity(Prefab),
                    Radius = Radius,
                    Count = Count
                };
                dstManager.AddSharedComponentData(entity, spawnerData);
            }

            // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
            public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
            {
                referencedPrefabs.Add(Prefab);
            }
        }
    }
}

