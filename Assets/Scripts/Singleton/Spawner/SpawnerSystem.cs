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
		GridSingleton _gridSingleton;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpawnDataSingleton>();
			// state.RequireForUpdate<YOUR_DATA_COMPONENT>();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<IdComponent>()
				.Build(ref state);
				
			_currentSeed = (uint) UnityEngine.Random.Range(0, uint.MaxValue);
			Random = Random.CreateFromIndex(_currentSeed);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			float deltaTime = SystemAPI.Time.DeltaTime;
			_spawnDataSingleton = SystemAPI.GetSingletonRW<SpawnDataSingleton>();
			_spawnDatas = SystemAPI.GetSingletonBuffer<SpawnDataBufferSingleton>();
			
			_gridSingleton = SystemAPI.GetSingleton<GridSingleton>();
			SystemAPI.TryGetSingletonEntity<GridSingleton>(out Entity partition);

			
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
			
			float2 pos = RandomPos(-_gridSingleton.HalfSize.x, _gridSingleton.HalfSize.x);
			float2 destination = RandomPos(-_gridSingleton.HalfSize.x, _gridSingleton.HalfSize.x);
			state.EntityManager.SetComponentData(spawned, new LocalTransform
			{
				Position = new float3(pos.x, 0, pos.y),
				Rotation = quaternion.identity,
				Scale = _spawnDataSingleton.ValueRO.Scale,
			});
			state.EntityManager.SetComponentData(spawned, new AgentPathComponent
			{
				Destination = destination,
			});
			
			float v = Random.NextFloat(_spawnDataSingleton.ValueRO.MinMaxRandomDeathDelay.x, _spawnDataSingleton.ValueRO.MinMaxRandomDeathDelay.y);
			state.EntityManager.SetComponentData(spawned, new Tertle.DestroyCleanup.DestroyByDurationComponent 
			{
				Duration = v,
				MaxDuration = v,
			});
			
			return spawned;
		}
		
		public float2 RandomPos(float min, float max)
		{
			return _gridSingleton.Origin + new float2(Random.NextFloat(min, max), Random.NextFloat(min, max));
		}
		
		public int2 RandomPos(int min, int max)
		{
			return Random.NextInt2(min, max);
		}
		
	}
