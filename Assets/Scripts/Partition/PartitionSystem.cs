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
		public Entity Entity;
		public float Radius;
		public ENUM_COLLIDER_LAYER Layer;
		
		public override string ToString()
		{
			return Pos.ToString() + " " + Entity.ToString();
		}
	}
	
	[UpdateInGroup(typeof(HashCoreSystemGroup))]
	public partial struct PartitionSystem : ISystem
	{
		public Random Random;
		private GridSingleton _gridSingleton;
		private SettingsSingleton _settingsSingleton;
		public Entity InGame;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyDataBufferSingleton>();
			state.RequireForUpdate<GridSingleton>();
			state.RequireForUpdate<IdComponent>();
			
		}
		
		public void OnDestroy()
		{
			// Hash.Dispose();
			
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// Log(currentSeed);
			
			SystemAPI.TryGetSingletonEntity<InGameSingleton>(out InGame);
			SystemAPI.TryGetSingleton(out _gridSingleton);
			SystemAPI.TryGetSingleton(out _settingsSingleton);
			SystemAPI.TryGetSingletonEntity<GridSingleton>(out Entity partition);
			// Log(SpawnDatas.IsCreated);
			
			var Hash = new NativeParallelMultiHashMap<int, Partition>(1024, Allocator.TempJob);
			// Log(Partitions.Length);
			
			// writing
			var writerJob = 
			new PartitionWriterJob()
			{
				HashMap = Hash.AsParallelWriter(),
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
			
			NativeList<HitData> bulletToEnemyHit = new(1024, Allocator.TempJob);
			var bulletToEnemyHandle = 
			new BulletToEnemyCollisionJob()
			{
				HashMap = Hash.AsReadOnly(),
				GridSingleton = _gridSingleton,
				HitDatas = bulletToEnemyHit.AsParallelWriter(),
				
			}.ScheduleParallel(writerJob);
			bulletToEnemyHandle.Complete();
			
			foreach (var hitData in bulletToEnemyHit)
			{
				// UnityEngine.Debug.Log(hitData.DistanceSq + "  \t" + hitData.Attacker + " " + hitData.Target);
			}
			UnityEngine.Debug.Log(bulletToEnemyHit.Length);
			bulletToEnemyHit.Dispose();
			
			// Log($"{Hash.Count()}/{Hash.Capacity} standard counter = {AllCounter} partitioned = {PartitionCounter}");
			Hash.Dispose();
		}
		
		[BurstCompile]
		public partial struct PartitionReaderJob : IJobEntity
		{
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Partition>.ReadOnly HashMap;
			
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			[BurstCompile]
			public void Execute(ref IdComponent data, AgentColliderComponent col, in LocalTransform ownerPos, [ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				NativeList<int> neighbors = new(GridSingleton.CalculateNeighborCount(col.RadiusInt), Allocator.Temp);
				neighbors.AddNoResize(data.PartitionId);
				GridSingleton.GetNeighborId(ref neighbors, data.PartitionId, col.RadiusInt);
				
				for (int i = 0; i < neighbors.Length; i++)
				{
					int key = neighbors[i];// + Check9[i];
					if (HashMap.TryGetFirstValue(key, out Partition neighbor, out var it))
					{
						do
						{
							if (neighbor.Entity == owner)
							{
								continue;
							}
							
							// test bullet with enemy
							if (col.Layer == ENUM_COLLIDER_LAYER.PlayerBullet && neighbor.Layer == ENUM_COLLIDER_LAYER.Enemy)
							{
								bool isCollided = math.distancesq(neighbor.Pos, ownerPos.Position.xz) > 1;
								
								#if DEBUG_PARTITION
								UnityEngine.Debug.DrawLine(
									new float3(neighbor.Pos.x, 0, neighbor.Pos.y), 
									new float3(ownerPos.Position.x , 0, ownerPos.Position.z), 
									isCollided ? UnityEngine.Color.white : UnityEngine.Color.magenta);
								#endif

								if (isCollided)
								{
									continue;
								}
								
							}
							
							
							// collide
							

						} while (HashMap.TryGetNextValue(out neighbor, ref it));
					}
				}
				
				neighbors.Dispose();
			}
		}
		
		[BurstCompile]
		public partial struct PartitionWriterJob : IJobEntity
		{
			public NativeParallelMultiHashMap<int, Partition>.ParallelWriter HashMap;
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			[BurstCompile]
			public void Execute(in LocalTransform localTransform, ref IdComponent data, in AgentColliderComponent col,
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
					Pos = pos,
					Entity = owner,
					Layer = col.Layer,
					Radius = col.Radius,
				});
				
				data.PartitionId = partitionId;
			}
		}

	}
}
