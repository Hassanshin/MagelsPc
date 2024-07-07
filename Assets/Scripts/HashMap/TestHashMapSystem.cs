using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
// using UnityEngine;
using Unity.Collections;
using Tertle.DestroyCleanup;

namespace Hash.HashMap
{
	public struct HashPos
	{
		public float2 Pos;
		public Entity Entity;
		public int2 Area;
		
		public override string ToString()
		{
			return Pos.ToString() + " " + Entity.ToString() + " " + Area.ToString();
		}
	}
	public partial struct TestHashMapSystem : ISystem
	{
		public NativeParallelMultiHashMap<int2, HashPos> Hash;
		public Random Random;
		public DynamicBuffer<SpawnDataBufferSingleton> SpawnDatas;
		public DynamicBuffer<AreaPartitionBuffer> AreaPartitionBuffers;
		public AreaPartitionSingleton AreaPartitionSingleton;
		public SpawnDataSingleton SpawnDataSingleton;
		public Entity InGame;
		public NativeList<float2> Partitions; 
		public int MaxSpawn;
		
		public int AllCounter;
		public int PartitionCounter;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpawnDataBufferSingleton>();
			state.RequireForUpdate<AreaPartitionSingleton>();
		}
		
		public void Log(object message)
		{
			UnityEngine.Debug.Log(message);
		}
		
		public float2 RandomPos(float min, float max)
		{
			return Random.NextFloat2Direction() * Random.NextFloat(min, max);
		}
		
		public int2 RandomPos(int min, int max)
		{
			return Random.NextInt2(min, max);
		}
		
		public Entity CreateEntity(ref SystemState state, float2 pos, int2 partitionId)
		{
			Entity spawned = state.EntityManager.Instantiate(SpawnDatas[0].Entity);
			
			state.EntityManager.SetComponentData(spawned, LocalTransform.FromPosition(new float3(pos.x, 0, pos.y)));
			state.EntityManager.SetComponentData(spawned, new EnemyIdComponent
			{
				PartitionId = partitionId,
			});
			return spawned;
		}
		
		public void OnDestroy()
		{
			Hash.Dispose();
			Partitions.Dispose();
			
		}

		// [BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			uint currentSeed = (uint) UnityEngine.Random.Range(0, uint.MaxValue);
			Random = Random.CreateFromIndex(currentSeed);
			Log(currentSeed);
			
			SystemAPI.TryGetSingletonEntity<InGameSingleton>(out InGame);
			SystemAPI.TryGetSingleton(out AreaPartitionSingleton);
			SpawnDatas = SystemAPI.GetBuffer<SpawnDataBufferSingleton>(InGame);
			SpawnDataSingleton = SystemAPI.GetSingleton<SpawnDataSingleton>();
			MaxSpawn = SpawnDataSingleton.MaxSpawnCount;
			
			SystemAPI.TryGetSingletonEntity<AreaPartitionSingleton>(out Entity partition);
			AreaPartitionBuffers = SystemAPI.GetBuffer<AreaPartitionBuffer>(partition);
			// Log(SpawnDatas.IsCreated);
			
			Hash = new NativeParallelMultiHashMap<int2, HashPos>(MaxSpawn, Allocator.Persistent);
			Partitions = new NativeList<float2>(AreaPartitionSingleton.Count, Allocator.Persistent);
			Log(Partitions.Length);
			
			for (int i = 0; i < MaxSpawn; i++)
			{
				float2 pos = RandomPos(-AreaPartitionSingleton.FullAreaSize.x * 0.5f, AreaPartitionSingleton.FullAreaSize.x * 0.5f);
				int2 partitionId = getAreaPartition(i, pos);
				Hash.Add(partitionId, new HashPos
				{
					Pos = pos,
					Entity = CreateEntity(ref state, pos, partitionId),
				});
				
				// UnityEngine.Debug.DrawLine(
				// 	new float3(pos.x, 0, pos.y), 
				// 	new float3(AreaPartitionBuffers[partitionId].Min.x + AreaPartitionSingleton.Spacing * 0.5f, 0, AreaPartitionBuffers[partitionId].Min.y + AreaPartitionSingleton.Spacing * 0.5f), 
				// 	UnityEngine.Color.black, 10);
			}
			
			
			int2[] check9 = new int2[] 
			{ 
				new (0, 0),
				
				new (0, 1),
				new (1, 0),
				new (0, -1),
				new (-1, 0),
				
				new (1, 1),
				new (1, -1),
				new (-1, 1),
				new (-1, -1),
			};
			
			EntityQueryDesc description = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<EnemyIdComponent>()
				}
			};
			EntityQuery query = state.GetEntityQuery(description);
			AllCounter = (query.CalculateEntityCount() - 1) * query.CalculateEntityCount();
			
			foreach (var (data, LocalTransform, dead, owner) in SystemAPI.Query<
				RefRO<EnemyIdComponent>, RefRO<LocalTransform>, EnabledRefRW<DestroyTag>
				>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
			{
				for (int i = 0; i < check9.Length; i++)
				{
					var neighborRegion = Hash.GetValuesForKey(data.ValueRO.PartitionId + check9[i]);
					foreach (HashPos key in neighborRegion)
					{
						// Log(data.ValueRO.PartitionId + check9[i] + "| " + owner + "| has " + key);
						if (key.Entity != owner)
						{
							PartitionCounter++;
							LocalTransform otherTransform = state.EntityManager.GetComponentData<LocalTransform>(key.Entity);
							UnityEngine.Debug.DrawLine(LocalTransform.ValueRO.Position, otherTransform.Position, UnityEngine.Color.red, 100);
						}
					}
				}
			}
            
			Log($"standard counter = {AllCounter} partitioned = {PartitionCounter}");
			state.Enabled = false;
		}

		private int2 getAreaPartition(int i, float2 pos)
		{
			float spacing = AreaPartitionSingleton.Spacing;
			
			return new int2((int)math.ceil(pos.x / spacing), (int)math.ceil(pos.y / spacing));
		}
	}
}
