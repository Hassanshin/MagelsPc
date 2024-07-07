using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

    public class InGameAuthoring : MonoBehaviour
    {
        public float Value = 1;
    }

    public class InGameAuthoringBaker : Baker<InGameAuthoring>
    {
        public override void Bake(InGameAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new InGameSingleton
            {
                Value = authoring.Value
            });
        }
    }

    public struct InGameSingleton : IComponentData
    {
        public float Value;
    }
