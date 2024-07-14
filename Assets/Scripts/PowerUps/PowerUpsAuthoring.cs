using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class PowerUpsAuthoring : MonoBehaviour
	{
		public PowerUpsComponent Comp = new()
		{
			Type = ENUM_POWER_UPS_TYPE.Health,
			Amount = 1,
		};
	}

	public class PowerUpsAuthoringBaker : Baker<PowerUpsAuthoring>
	{
		public override void Bake(PowerUpsAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, authoring.Comp);
		}
	}
}

	[System.Serializable]
	public struct PowerUpsComponent : IComponentData
	{
		public ENUM_POWER_UPS_TYPE Type;
		public int Amount;
	}
