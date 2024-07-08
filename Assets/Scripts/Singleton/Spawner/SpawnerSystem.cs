using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

public partial struct SpawnerSystem : ISystem
	{
		public Random Random;
		EntityQuery _query;
		RefRW<SpawnDataSingleton> _spawnDataSingleton;
		uint _currentSeed;
		DynamicBuffer<SpawnDataBufferSingleton> _spawnDatas;
		AreaPartitionSingleton _areaPartitionSingleton;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpawnDataSingleton>();
			// state.RequireForUpdate<YOUR_DATA_COMPONENT>();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EnemyIdComponent>()
				.Build(ref state);
				
			_currentSeed = (uint) UnityEngine.Random.Range(0, uint.MaxValue);
			Random = Random.CreateFromIndex(_currentSeed);
		}

		// [BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			float deltaTime = SystemAPI.Time.DeltaTime;
			_spawnDataSingleton = SystemAPI.GetSingletonRW<SpawnDataSingleton>();
			_spawnDatas = SystemAPI.GetSingletonBuffer<SpawnDataBufferSingleton>();
			_areaPartitionSingleton = SystemAPI.GetSingleton<AreaPartitionSingleton>();
			
			
			int currentCount = _query.CalculateEntityCount();
			_spawnDataSingleton.ValueRW.CurrentCount = currentCount;
			
			int deltaSpawn = _spawnDataSingleton.ValueRO.MaxSpawnCount - currentCount;
			for (int i = 0; i < deltaSpawn; i++)
			{
				Entity spawned = CreateEntity(ref state);
				
			}
		}
		
		public Entity CreateEntity(ref SystemState state)
		{
			Entity spawned = state.EntityManager.Instantiate(_spawnDatas[0].Entity);
			
			float2 pos = RandomPos(-_areaPartitionSingleton.FullAreaSize.x * 0.5f, _areaPartitionSingleton.FullAreaSize.x * 0.5f);
			state.EntityManager.SetComponentData(spawned, LocalTransform.FromPosition(new float3(pos.x, 0, pos.y)));
			
			float v = Random.NextFloat(1, 5);
			state.EntityManager.SetComponentData(spawned, new Tertle.DestroyCleanup.DestroyByDurationComponent 
			{
				Duration = v,
				MaxDuration = v,
			});
			
			return spawned;
		}
		
		public float2 RandomPos(float min, float max)
		{
			return new float2(Random.NextFloat(min, max), Random.NextFloat(min, max));
		}
		
		public int2 RandomPos(int min, int max)
		{
			return Random.NextInt2(min, max);
		}
		
	}
