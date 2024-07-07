using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

    public class EnemyAuthoring : MonoBehaviour
    {
        public float Value;
    }

    public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new EnemyAuthoringComponent
            {
                Value = authoring.Value
            });
        }
    }

    public struct EnemyAuthoringComponent : IComponentData
    {
        public float Value;
    }
