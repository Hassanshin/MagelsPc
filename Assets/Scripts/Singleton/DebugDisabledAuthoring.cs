using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
    public class DebugDisabledAuthoring : MonoBehaviour
    {
    }

    public class DebugDisabledAuthoringBaker : Baker<DebugDisabledAuthoring>
    {
        public override void Bake(DebugDisabledAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new DebugDisabledTag
            {
            });
        }
    }
}

    public struct DebugDisabledTag : IComponentData
    {
    }
