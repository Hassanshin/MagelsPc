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
		public DynamicBuffer<AreaPartitionBuffer> AreaPartitionBuffers;
		public AreaPartitionSingleton AreaPartitionSingleton;
		public Entity InGame;
		NativeList<int2> _check9;
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
			// Hash.Dispose();
			_check9.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// Log(currentSeed);
			
			SystemAPI.TryGetSingletonEntity<InGameSingleton>(out InGame);
			SystemAPI.TryGetSingleton(out AreaPartitionSingleton);
			SystemAPI.TryGetSingletonEntity<AreaPartitionSingleton>(out Entity partition);
			// Log(SpawnDatas.IsCreated);
			
			var Hash = new NativeParallelMultiHashMap<int2, HashPos>(1024, Allocator.TempJob);
			// Log(Partitions.Length);
			
			// writing
			var writerJob = new PartitionWriterJob()
			{
				HashMap = Hash.AsParallelWriter(),
				Spacing = AreaPartitionSingleton.Spacing,
			}.ScheduleParallel(state.Dependency);
			writerJob.Complete();
			
			// reading
			var readerJob = new PartitionReaderJob()
			{
				Check9 = _check9,
				HashMap = Hash.AsReadOnly(),
				// TransformLookup = TransformLookup,
				
			}.ScheduleParallel(state.Dependency);
			readerJob.Complete();
			
			// Log($"{Hash.Count()}/{Hash.Capacity} standard counter = {AllCounter} partitioned = {PartitionCounter}");
			Hash.Dispose();
		}
		
		
		[BurstCompile]
		public partial struct PartitionReaderJob : IJobEntity
		{
			[ReadOnly]
			public NativeParallelMultiHashMap<int2, HashPos>.ReadOnly HashMap;
			[ReadOnly]
			public NativeList<int2> Check9;
			
			[BurstCompile]
			public void Execute(ref EnemyIdComponent data, in LocalTransform ownerPos, [ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				for (int i = 0; i < Check9.Length; i++)
				{
					int2 key = data.PartitionId + Check9[i];
					
					if (HashMap.TryGetFirstValue(key, out HashPos neighbor, out var it))
                    {
                        do
                        {
                            if (neighbor.Entity == owner)
                            {
                                continue;
                            }
                            
                            // UnityEngine.Debug.DrawLine(
                            //     new float3(neighbor.Pos.x, 0, neighbor.Pos.y), 
                            //     new float3(ownerPos.Position.x , 0, ownerPos.Position.z), 
                            //     UnityEngine.Color.yellow);
                            
                            if (math.distancesq(neighbor.Pos, ownerPos.Position.xz) > 1)
                            {
                                continue;
                            }
                            
                            // collide
                            

                        } while (HashMap.TryGetNextValue(out neighbor, ref it));
                    }
				}
			}
		}
		
		[BurstCompile]
		public partial struct PartitionWriterJob : IJobEntity
		{
			public NativeParallelMultiHashMap<int2, HashPos>.ParallelWriter HashMap;
			public float Spacing;
			
			[BurstCompile]
			public void Execute(in LocalTransform localTransform, ref EnemyIdComponent data,
				[ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				float2 pos = localTransform.Position.xz;
				int2 partitionId = getAreaPartition(pos);
				// UnityEngine.Debug.Log(partitionId + " " + pos);
				HashMap.Add(partitionId, new HashPos
				{
					Pos = pos,
					Entity = owner,
				});
				
				data.PartitionId = partitionId;
			}
			
			private int2 getAreaPartition(float2 pos)
			{
				return new int2((int)math.ceil(pos.x / Spacing), (int)math.ceil(pos.y / Spacing));
			}
		}

	}
}
