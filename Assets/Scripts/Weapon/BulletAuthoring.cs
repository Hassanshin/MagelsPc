using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class BulletAuthoring : MonoBehaviour
	{
		public int Damage;
		public int Pierce;
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
	}
