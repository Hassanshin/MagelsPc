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
		public Random Random;
		private GridSingleton _gridSingleton;
		public Entity InGame;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpawnDataBufferSingleton>();
			state.RequireForUpdate<GridSingleton>();
			state.RequireForUpdate<EnemyIdComponent>();
			
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
			SystemAPI.TryGetSingletonEntity<GridSingleton>(out Entity partition);
			// Log(SpawnDatas.IsCreated);
			
			var Hash = new NativeParallelMultiHashMap<int, HashPos>(1024, Allocator.TempJob);
			// Log(Partitions.Length);
			
			// writing
			var writerJob = new PartitionWriterJob()
			{
				HashMap = Hash.AsParallelWriter(),
				GridSingleton = _gridSingleton,
			}.ScheduleParallel(state.Dependency);
			writerJob.Complete();
			
			// reading
			var readerJob = new PartitionReaderJob()
			{
				HashMap = Hash.AsReadOnly(),
				GridSingleton = _gridSingleton,
			}.ScheduleParallel(state.Dependency);
			readerJob.Complete();
			
			// Log($"{Hash.Count()}/{Hash.Capacity} standard counter = {AllCounter} partitioned = {PartitionCounter}");
			Hash.Dispose();
		}
		
		
		[BurstCompile]
		public partial struct PartitionReaderJob : IJobEntity
		{
			[ReadOnly]
			public NativeParallelMultiHashMap<int, HashPos>.ReadOnly HashMap;
			
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			[BurstCompile]
			public void Execute(ref EnemyIdComponent data, in LocalTransform ownerPos, [ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				int checkRadius = 2;
				NativeList<int> neighbors = new(GridSingleton.CalculateNeighborCount(checkRadius), Allocator.Temp);
				neighbors.AddNoResize(data.PartitionId);
				GridSingleton.GetNeighborId(ref neighbors, data.PartitionId, checkRadius);
				
				for (int i = 0; i < neighbors.Length; i++)
				{
					int key = neighbors[i];// + Check9[i];
					if (HashMap.TryGetFirstValue(key, out HashPos neighbor, out var it))
					{
						do
						{
							if (neighbor.Entity == owner)
							{
								continue;
							}
							
							// UnityEngine.Debug.DrawLine(
							// 	new float3(neighbor.Pos.x, 0, neighbor.Pos.y), 
							// 	new float3(ownerPos.Position.x , 0, ownerPos.Position.z), 
							// 	UnityEngine.Color.yellow);
							
							if (math.distancesq(neighbor.Pos, ownerPos.Position.xz) > 1)
							{
								continue;
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
			public NativeParallelMultiHashMap<int, HashPos>.ParallelWriter HashMap;
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			[BurstCompile]
			public void Execute(in LocalTransform localTransform, ref EnemyIdComponent data,
				[ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				float2 pos = localTransform.Position.xz;
				int partitionId = GridSingleton.GetIdFromPos(pos);
				// UnityEngine.Debug.Log(partitionId + " " + pos);
				HashMap.Add(partitionId, new HashPos
				{
					Pos = pos,
					Entity = owner,
				});
				
				data.PartitionId = partitionId;
			}
		}

	}
}
