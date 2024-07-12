using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
    public class PlayerAuthoring : MonoBehaviour
    {
    }

    public class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new PlayerTag
            {
            });
        }
    }
}

    public struct PlayerTag : IComponentData
    {
    }
