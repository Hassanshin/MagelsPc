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
		public DynamicBuffer<AreaPartitionBuffer> AreaPartitionBuffers;
		public AreaPartitionSingleton AreaPartitionSingleton;
		public SpawnDataSingleton SpawnDataSingleton;
		public Entity InGame;
		public int MaxSpawn;
		
		public int AllCounter;
		public int PartitionCounter;
		NativeList<int2> _check9;
		uint _currentSeed;
		public EntityQuery Query;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpawnDataBufferSingleton>();
			state.RequireForUpdate<AreaPartitionSingleton>();
			
			_check9 = new NativeList<int2>(9, Allocator.Persistent);
			_check9.AddNoResize(new int2(0, 0));
			_check9.AddNoResize(new int2(0, 1));
			_check9.AddNoResize(new int2(1, 0));
			_check9.AddNoResize(new int2(0, -1));
			_check9.AddNoResize(new int2(-1, 0));
			_check9.AddNoResize(new int2(1, 1));
			_check9.AddNoResize(new int2(1, -1));
			_check9.AddNoResize(new int2(-1, 1));
			_check9.AddNoResize(new int2(-1, -1));
		}
		
		public void OnDestroy()
		{
			Hash.Dispose();
			_check9.Dispose();
		}

		// [BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// Log(currentSeed);
			
			SystemAPI.TryGetSingletonEntity<InGameSingleton>(out InGame);
			SystemAPI.TryGetSingleton(out AreaPartitionSingleton);
			
			SystemAPI.TryGetSingletonEntity<AreaPartitionSingleton>(out Entity partition);
			// Log(SpawnDatas.IsCreated);
			
			Hash = new NativeParallelMultiHashMap<int2, HashPos>(MaxSpawn, Allocator.Temp);
			// Log(Partitions.Length);
			
			// writing
			foreach (var (data, localTransform, dead, owner) in SystemAPI.Query<
				RefRW<EnemyIdComponent>, RefRO<LocalTransform>, EnabledRefRW<DestroyTag>
				>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
			{
				float2 pos = localTransform.ValueRO.Position.xz;
				int2 partitionId = getAreaPartition(pos);
				// UnityEngine.Debug.Log(partitionId + " " + pos);
				Hash.Add(partitionId, new HashPos
				{
					Pos = pos,
					Entity = owner,
				});
				
				data.ValueRW.PartitionId = partitionId;
			}
			
			UnityEngine.Debug.Log("" + Hash.Count());
			
			// reading
			foreach (var (data, LocalTransform, dead, owner) in SystemAPI.Query<
				RefRO<EnemyIdComponent>, RefRO<LocalTransform>, EnabledRefRW<DestroyTag>
				>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
			{
				for (int i = 0; i < _check9.Length; i++)
				{
					var neighborRegion = Hash.GetValuesForKey(data.ValueRO.PartitionId + _check9[i]);
					foreach (HashPos key in neighborRegion)
					{
						// UnityEngine.Debug.Log(data.ValueRO.PartitionId + _check9[i] + "| " + owner + "| has " + key);
						if (key.Entity != owner)
						{
							PartitionCounter++;
							LocalTransform otherTransform = state.EntityManager.GetComponentData<LocalTransform>(key.Entity);
							UnityEngine.Debug.DrawLine(LocalTransform.ValueRO.Position, otherTransform.Position, UnityEngine.Color.red);
						}
					}
				}
			}
			
			// Log($"{Hash.Count()}/{Hash.Capacity} standard counter = {AllCounter} partitioned = {PartitionCounter}");
			Hash.Dispose();
		}

		private int2 getAreaPartition(float2 pos)
		{
			float spacing = AreaPartitionSingleton.Spacing;
			
			return new int2((int)math.ceil(pos.x / spacing), (int)math.ceil(pos.y / spacing));
		}
	}
}
