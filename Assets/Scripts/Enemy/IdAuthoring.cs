using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class IdAuthoring : MonoBehaviour
	{
		public int Id = -1;
	}

	public class EnemyIdAuthoringBaker : Baker<IdAuthoring>
	{
		public override void Bake(IdAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new IdComponent
			{
				Id = authoring.Id
			});
		}
	}
}

	public struct IdComponent : IComponentData
	{
		public int Id;
		public int PartitionId;
	}
