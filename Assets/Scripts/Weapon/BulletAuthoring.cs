using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class BulletAuthoring : MonoBehaviour
	{
		public int Damage = 15;
		public int Pierce = 12;
		public float DelayBetweenHits = 0.5f;
	}

	public class BulletAuthoringBaker : Baker<BulletAuthoring>
	{
		public override void Bake(BulletAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new BulletComponent
			{
				Damage = authoring.Damage,
				Pierce = authoring.Pierce,
				DelayBetweenHits = authoring.DelayBetweenHits,
			});
			
			AddComponent<BulletTag>(entity);
		}
	}
}
	public struct BulletTag : IComponentData { }

	public struct BulletComponent : IComponentData
	{
		public int Damage;
		public int Pierce;
		public float DelayBetweenHits;
	}
