using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
    public class AgentAvoidanceAuthoring : MonoBehaviour
    {
        public float Radius;
    }

    public class AgentAvoidanceAuthoringBaker : Baker<AgentAvoidanceAuthoring>
    {
        public override void Bake(AgentAvoidanceAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new AgentAvoidanceAuthoringComponent
            {
                Radius = authoring.Radius
            });
        }
    }
}

    public struct AgentAvoidanceAuthoringComponent : IComponentData
    {
        public float Radius;
    }
