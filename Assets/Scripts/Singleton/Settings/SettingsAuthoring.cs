using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
    public class SettingsAuthoring : MonoBehaviour
    {
        public bool DebugShowPartition;
        public bool DebugShowPathFinding;
    }

    public class SettingsAuthoringBaker : Baker<SettingsAuthoring>
    {
        public override void Bake(SettingsAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            
            AddComponent(entity, new SettingsSingleton
            {
                DebugShowPartition = authoring.DebugShowPartition,
                DebugShowPathFinding = authoring.DebugShowPathFinding,
            });
        }
    }
}

    public struct SettingsSingleton : IComponentData
    {
        public bool DebugShowPartition;
        public bool DebugShowPathFinding;
    }
