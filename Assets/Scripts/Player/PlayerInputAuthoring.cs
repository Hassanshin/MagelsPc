using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
    public class PlayerInputAuthoring : MonoBehaviour
    {
    }

    public class PlayerInputAuthoringBaker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new PlayerInputAuthoringComponent
            {
            });
        }
    }
}

    public struct PlayerInputAuthoringComponent : IComponentData
    {
        public float2 Input;
    }
