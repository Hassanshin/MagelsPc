using Hash.Stats;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class StatsAuthoring : MonoBehaviour
	{
		public StatsData Data = new()
		{
			Health = Stats.InitHealth(100),
			Attack = Stats.Init(3),
			Mana = Stats.InitHealth(50),
		};
	}

	public class StatsAuthoringBaker : Baker<StatsAuthoring>
	{
		public override void Bake(StatsAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new StatsComponent
			{
				Data = authoring.Data
			});
		}
	}
}
	[System.Serializable]
	public struct StatsData
	{
		public Stats Health;
		public Stats Mana;
		public Stats Attack;
	}
	
	public struct StatsComponent : IComponentData
	{
		public StatsData Data;
	}
