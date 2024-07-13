using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class SpawnDataAuthoring : MonoBehaviour
	{
		public int MaxSpawnCount = 10;
		public EnemyAuthoring[] EnemyList;
		public BulletAuthoring[] BulletList;
		public float2 MinMaxRandomDeathDelay = new float2(1, 10);
		public float Scale = 1;
	}

	public class SpawnDataAuthoringBaker : Baker<SpawnDataAuthoring>
	{
		public override void Bake(SpawnDataAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new SpawnDataSingleton
			{
				MaxSpawnCount = authoring.MaxSpawnCount,
				Scale = authoring.Scale,
				MinMaxRandomDeathDelay = authoring.MinMaxRandomDeathDelay,
			});
			
			bakeEnemyBuffer(entity, authoring.EnemyList, TransformUsageFlags.Dynamic);
			bakeBulletBuffer(entity, authoring.BulletList, TransformUsageFlags.Dynamic);
		}
		
		private void bakeEnemyBuffer(Entity entity, EnemyAuthoring[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<EnemyDataBufferSingleton> buffer = AddBuffer<EnemyDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new EnemyDataBufferSingleton { Entity = convertToEntity };	
			}
		}
		
		private void bakeBulletBuffer(Entity entity, BulletAuthoring[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<BulletDataBufferSingleton> buffer = AddBuffer<BulletDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new BulletDataBufferSingleton { Entity = convertToEntity };	
			}
		}
	}
}
	
	
	public struct BulletDataBufferSingleton : IBufferElementData
	{
		public Entity Entity;
	}
	
	public struct EnemyDataBufferSingleton : IBufferElementData
	{
		public Entity Entity;
	}
	
	public struct SpawnDataSingleton : IComponentData
	{
		public int MaxSpawnCount;
		public int CurrentCount;
		public float Scale;
		public float2 MinMaxRandomDeathDelay;
	}
