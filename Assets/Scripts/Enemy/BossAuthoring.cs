using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class BossAuthoring : MonoBehaviour
	{
		public BossTag Comp = new()
		{
		};
	}

	public class BossAuthoringBaker : Baker<BossAuthoring>
	{
		public override void Bake(BossAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, authoring.Comp);
		}
	}
}

	[System.Serializable]
	public struct BossTag : IComponentData
	{
	}
