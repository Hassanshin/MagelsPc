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
		public GameObject[] WallList;
		public GameObject[] DataBlockList;
		public PowerUpsAuthoring[] PowerUpsList;
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
			
			bakeWallBuffer(entity, authoring.WallList, TransformUsageFlags.None );
			bakeDataBlockBuffer(entity, authoring.DataBlockList, TransformUsageFlags.None );
			
			bakeDataPowerUpsBuffer(entity, authoring.PowerUpsList, TransformUsageFlags.Dynamic );
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
		private void bakeWallBuffer(Entity entity, GameObject[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<WallDataBufferSingleton> buffer = AddBuffer<WallDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new WallDataBufferSingleton { Entity = convertToEntity };	
			}
		}
		private void bakeDataBlockBuffer(Entity entity, GameObject[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<DataBlockDataBufferSingleton> buffer = AddBuffer<DataBlockDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new DataBlockDataBufferSingleton { Entity = convertToEntity };	
			}
		}
		private void bakeDataPowerUpsBuffer(Entity entity, PowerUpsAuthoring[] prefabs, TransformUsageFlags flags)
		{
			DynamicBuffer<PowerUpsDataBufferSingleton> buffer = AddBuffer<PowerUpsDataBufferSingleton>(entity);
			buffer.Length = prefabs.Length;
			
			for (int i = 0; i < prefabs.Length; i++)
			{
				Entity convertToEntity = GetEntity(prefabs[i], flags);

				buffer[i] = new PowerUpsDataBufferSingleton { Entity = convertToEntity };	
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
	public struct WallDataBufferSingleton : IBufferElementData
	{
		public Entity Entity;
	}
	public struct DataBlockDataBufferSingleton : IBufferElementData
	{
		public Entity Entity;
	}
	public struct PowerUpsDataBufferSingleton : IBufferElementData
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
