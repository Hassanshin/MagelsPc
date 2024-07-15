using Hash.Stats;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class StatsAuthoring : MonoBehaviour
	{
		public StatsComponent Data = new()
		{
			Health = Stats.InitHealth(100),
			Attack = Stats.Init(3),
			Energy = Stats.InitHealth(50),
		};
	}

	public class StatsAuthoringBaker : Baker<StatsAuthoring>
	{
		public override void Bake(StatsAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, authoring.Data);
			AddComponent<IFrameComponent>(entity);
		}
	}
}
	
	[System.Serializable]
	public struct StatsComponent : IComponentData
	{
		public Stats Health;
		public Stats Energy;
		public Stats Attack;
	}
	
	public struct IFrameComponent : IComponentData
	{
		public readonly bool IsOnIFrame => CurrentDuration > 0;
		public float CurrentDuration;
	}
