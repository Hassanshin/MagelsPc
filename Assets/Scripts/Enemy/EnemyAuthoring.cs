using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

    public class EnemyAuthoring : MonoBehaviour
    {
    }

    public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new EnemyTag
            {
            });
        }
    }

    public struct EnemyTag : IComponentData
    {
    }
