#define DEBUG_PARTITION
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
using Hash.Util;

namespace Hash.HashMap
{
	public struct Partition
	{
		public float2 Pos;
		public quaternion Rot;
		public Entity Entity;
		public AgentColliderComponent Collider;
		
		public override string ToString()
		{
			return Pos.ToString() + " " + Entity.ToString();
		}
	}
	
	[System.Serializable]
	public struct HitData 
	{
		public float2 Pos;
		public bool IsKilling;
		public Entity Target;
		public Entity Attacker;
		public float DistanceSq;
		public float CurrentDuration;
		public ENUM_COLLIDER_LAYER TargetLayer;
		public override string ToString()
		{
			return $"{Attacker} \t-> {Target} \t| {CurrentDuration}";
		}
		
	}
	
	[UpdateInGroup(typeof(HashCoreSystemGroup))]
	public partial struct PartitionSystem : ISystem
	{
		private const int INITIAL_CAPACITY = 2048;
		public Random Random;
		private GridSingleton _gridSingleton;
		private SettingsSingleton _settingsSingleton;
		public Entity InGame;
		public NativeParallelMultiHashMap<Entity, HitData> HitDataHashMap;
		public float DeltaTime;
		public ComponentLookup<StatsComponent> StatsComponentLookup;
		public ComponentLookup<BulletComponent> BulletComponentLookup;
		public DynamicBuffer<HitBufferDataMono> HitBufferMono;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyDataBufferSingleton>();
			state.RequireForUpdate<GridSingleton>();
			state.RequireForUpdate<IdComponent>();
			
			StatsComponentLookup = SystemAPI.GetComponentLookup<StatsComponent>();
			BulletComponentLookup = SystemAPI.GetComponentLookup<BulletComponent>();
			
			HitDataHashMap = new NativeParallelMultiHashMap<Entity, HitData>(INITIAL_CAPACITY, Allocator.Persistent);
		}
		
		public void OnDestroy()
		{
			// Hash.Dispose();
			HitDataHashMap.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// Log(currentSeed);
			
			SystemAPI.TryGetSingletonEntity<InGameSingleton>(out InGame);
			SystemAPI.TryGetSingleton(out _gridSingleton);
			SystemAPI.TryGetSingleton(out _settingsSingleton);
			SystemAPI.TryGetSingletonEntity<GridSingleton>(value: out Entity partition);
			SystemAPI.TryGetSingletonBuffer(out HitBufferMono);
			
			StatsComponentLookup.Update(ref state);
			BulletComponentLookup.Update(ref state);
			
			DeltaTime = SystemAPI.Time.DeltaTime;
			// Log(SpawnDatas.IsCreated);
			
			HitDataHashMap = DecayHitData();
			
			// define partition
			var PartitionHashMap = new NativeParallelMultiHashMap<int, Partition>(INITIAL_CAPACITY, Allocator.TempJob);
			
			// writing
			var writerJob = 
			new PartitionWriteIdJob()
			{
				HashMap = PartitionHashMap.AsParallelWriter(),
				GridSingleton = _gridSingleton,
			}.ScheduleParallel(state.Dependency);
			writerJob.Complete();
			
			// reading
			// var readerJob = 
			// new PartitionReaderJob()
			// {
			// 	HashMap = Hash.AsReadOnly(),
			// 	GridSingleton = _gridSingleton,
			// }.ScheduleParallel(writerJob);
			// readerJob.Complete();
			
			NativeList<HitData> newHitDataList = new(INITIAL_CAPACITY, Allocator.TempJob);
			var bulletToEnemyHandle = 
			new BulletToEnemyCollisionJob()
			{
				GridSingleton = _gridSingleton,
				PartitionHashMap = PartitionHashMap.AsReadOnly(),
				
				HitDataHashMap = HitDataHashMap.AsReadOnly(),
				NewHitDataList = newHitDataList.AsParallelWriter(),
				
			}.ScheduleParallel(writerJob);
			bulletToEnemyHandle.Complete();
			
			for (int i = 0; i < newHitDataList.Length; i++)
			{
				HitData hitData = newHitDataList[i];
				HitDataHashMap.Add(hitData.Attacker, hitData);
				// UnityEngine.Debug.Log(hitData.DistanceSq + "  \t" + hitData.Attacker + " " + hitData.Target);
				
				var bullet = BulletComponentLookup.GetRefRW(hitData.Attacker);
				if (hitData.TargetLayer == ENUM_COLLIDER_LAYER.Enemy)
				{
					var stats = StatsComponentLookup.GetRefRW(hitData.Target);
					processBulletHit(ref state, hitData, stats, bullet);
					hitData.IsKilling = stats.ValueRO.Data.Health.Value <= 0;
					UnityEngine.Debug.Log(hitData.IsKilling);
				}
				else if(hitData.TargetLayer == ENUM_COLLIDER_LAYER.Wall)
				{
					state.EntityManager.SetComponentEnabled<DestroyTag>(hitData.Attacker, true);
				}
				
				// to be read by mono
				HitBufferMono.Add(new HitBufferDataMono
				{
					Hit = hitData,
					Weapon = bullet.ValueRO.Weapon,
				});
			}
			newHitDataList.Dispose();
			
			// Log($"{Hash.Count()}/{Hash.Capacity} standard counter = {AllCounter} partitioned = {PartitionCounter}");
			PartitionHashMap.Dispose();
		}

		private void processBulletHit(ref SystemState state, HitData hitData, RefRW<StatsComponent> stats, RefRW<BulletComponent> bullet)
		{
			if (stats.ValueRO.Data.Health.Value <= 0 || bullet.ValueRO.Pierce <= 0)
			{
				return;
			}
			
			stats.ValueRW.Data.Health.Add(-bullet.ValueRO.Damage);
			
			if (stats.ValueRO.Data.Health.Value <= 0)
			{
				// target die. drop items?
				state.EntityManager.SetComponentEnabled<DestroyTag>(hitData.Target, true);
			}
			
			bullet.ValueRW.Pierce--;
			if (bullet.ValueRO.Pierce <= 0)
			{
				state.EntityManager.SetComponentEnabled<DestroyTag>(hitData.Attacker, true);
			}
		}

		public NativeParallelMultiHashMap<Entity, HitData> DecayHitData()
		{
			var updatedHitDataHashMap = new NativeParallelMultiHashMap<Entity, HitData>(HitDataHashMap.Capacity, Allocator.TempJob);
			 // Create an enumerator to iterate through all key-value pairs in the original hashmap
			var enumerator = HitDataHashMap.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var key = enumerator.Current.Key;
				var hitData = enumerator.Current.Value;

				// Reduce the current duration
				hitData.CurrentDuration -= DeltaTime;
				
				// If current duration is greater than 0, add it to the updated hashmap
				if (hitData.CurrentDuration > 0)
				{
					updatedHitDataHashMap.Add(key, hitData);
				}
			}
			
			enumerator.Dispose();
			HitDataHashMap.Dispose();
			
			return updatedHitDataHashMap;
		}
		
		// [BurstCompile]
		// public partial struct PartitionReaderJob : IJobEntity
		// {
		// 	[ReadOnly]
		// 	public NativeParallelMultiHashMap<int, Partition>.ReadOnly HashMap;
			
		// 	[ReadOnly]
		// 	public GridSingleton GridSingleton;
			
		// 	[BurstCompile]
		// 	public void Execute(ref IdComponent data, AgentColliderComponent col, in LocalTransform ownerPos, [ChunkIndexInQuery] int chunkIndex, Entity owner)
		// 	{
		// 		NativeList<int> neighbors = new(GridSingleton.CalculateNeighborCount(col.RadiusInt), Allocator.Temp);
		// 		neighbors.AddNoResize(data.PartitionId);
		// 		GridSingleton.GetNeighborId(ref neighbors, data.PartitionId, col.RadiusInt);
				
		// 		for (int i = 0; i < neighbors.Length; i++)
		// 		{
		// 			int key = neighbors[i];// + Check9[i];
		// 			if (HashMap.TryGetFirstValue(key, out Partition neighbor, out var it))
		// 			{
		// 				do
		// 				{
		// 					if (neighbor.Entity == owner)
		// 					{
		// 						continue;
		// 					}
							
		// 					bool isCollided = true;
		// 					#if DEBUG_PARTITION
		// 						UnityEngine.Debug.DrawLine(
		// 							new float3(neighbor.Pos.x, 0, neighbor.Pos.y), 
		// 							new float3(ownerPos.Position.x , 0, ownerPos.Position.z), 
		// 							isCollided ? UnityEngine.Color.white : UnityEngine.Color.magenta);
		// 					#endif

		// 					if (isCollided)
		// 					{
		// 						continue;
		// 					}
		// 					// collide
							

		// 				} while (HashMap.TryGetNextValue(out neighbor, ref it));
		// 			}
		// 		}
				
		// 		neighbors.Dispose();
		// 	}
		// }
		
		[BurstCompile]
		public partial struct PartitionWriteIdJob : IJobEntity
		{
			public NativeParallelMultiHashMap<int, Partition>.ParallelWriter HashMap;
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			[BurstCompile]
			public void Execute(LocalToWorld localTransform, ref IdComponent data, in AgentColliderComponent col,
				[ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				float2 pos = localTransform.Position.xz;
				if (!GridSingleton.IsOnValidGrid(pos))
				{
					return;
				}
				
				int partitionId = GridSingleton.GetIdFromPos(pos);
				// UnityEngine.Debug.Log(partitionId + " " + pos);
				HashMap.Add(partitionId, new Partition
				{
					Entity = owner,
					Pos = localTransform.Position.xz,
					Rot = localTransform.Rotation,
					Collider = col,
				});
				
				data.PartitionId = partitionId;
			}
		}

	}
}
