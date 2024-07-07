using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

	public class EnemyIdAuthoring : MonoBehaviour
	{
		public int Id = -1;
	}

	public class EnemyIdAuthoringBaker : Baker<EnemyIdAuthoring>
	{
		public override void Bake(EnemyIdAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new EnemyIdComponent
			{
				Id = authoring.Id
			});
		}
	}

	public struct EnemyIdComponent : IComponentData
	{
		public int Id;
		public int2 PartitionId;
	}
