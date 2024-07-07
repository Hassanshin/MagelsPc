using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

	public class SpawnDataAuthoring : MonoBehaviour
	{
		public int MaxSpawnCount = 10;
		public GameObject[] EnemyList;
	}

	public class SpawnDataAuthoringBaker : Baker<SpawnDataAuthoring>
	{
		public override void Bake(SpawnDataAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new SpawnDataSingleton
			{
				MaxSpawnCount = authoring.MaxSpawnCount,
			});
			
			bakeBuffer(entity, authoring.EnemyList, TransformUsageFlags.Dynamic);
		}
		private void bakeBuffer(Entity entity, GameObject[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<SpawnDataBufferSingleton> buffer = AddBuffer<SpawnDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new SpawnDataBufferSingleton { Entity = convertToEntity };	
			}
		}
	}
	
	public struct SpawnDataBufferSingleton : IBufferElementData
	{
		public Entity Entity;
	}
	
	public struct SpawnDataSingleton : IComponentData
	{
		public int MaxSpawnCount;
	}
