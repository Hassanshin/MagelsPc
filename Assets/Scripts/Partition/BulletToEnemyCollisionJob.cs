#define DEBUG_PARTITION
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
// using UnityEngine;
using Unity.Collections;
using System;

namespace Hash.HashMap
{
	public partial struct PartitionSystem
	{
		[BurstCompile]
		public partial struct BulletToEnemyCollisionJob : IJobEntity
		{
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Partition>.ReadOnly PartitionHashMap;
			[ReadOnly]
			public NativeParallelMultiHashMap<Entity, HitData>.ReadOnly HitDataHashMap;
			
			[ReadOnly]
			public GridSingleton GridSingleton;
			
			public NativeList<HitData>.ParallelWriter NewHitDataList;
			
			[BurstCompile]
			public void Execute(ref IdComponent data, BulletComponent bullet, AgentColliderComponent col, in LocalTransform ownerPos, [ChunkIndexInQuery] int chunkIndex, Entity owner)
			{
				NativeList<int> neighbors = new(GridSingleton.CalculateNeighborCount(col.RadiusInt), Allocator.Temp);
				neighbors.AddNoResize(data.PartitionId);
				GridSingleton.GetNeighborId(ref neighbors, data.PartitionId, col.RadiusInt);
				
				// check every neighbor
				for (int i = 0; i < neighbors.Length; i++)
				{
					int key = neighbors[i];
					
					// check every room entity
					if (PartitionHashMap.TryGetFirstValue(key, out Partition neighbor, out var it))
					{
						do
						{
							if (neighbor.Entity == owner)
							{
								continue;
							}

							// test bullet with enemy
							if (col.Layer != ENUM_COLLIDER_LAYER.PlayerBullet || neighbor.Layer != ENUM_COLLIDER_LAYER.Enemy)
							{
								continue;
							}

							//check if hit earlier
							if (alreadyHit(owner, neighbor.Entity))
							{
								continue;
							}

							float distancesq = math.distancesq(neighbor.Pos, ownerPos.Position.xz);
							float combinedRadius = col.Radius + neighbor.Radius;
							bool isCollided = distancesq <= math.pow(combinedRadius, 2);

							#if DEBUG_PARTITION
							UnityEngine.Debug.DrawLine(
								new float3(neighbor.Pos.x, 0, neighbor.Pos.y),
								new float3(ownerPos.Position.x, 0, ownerPos.Position.z),
								isCollided ? UnityEngine.Color.magenta : UnityEngine.Color.white);
							#endif
							
							if (!isCollided)
							{
								continue;
							}
							
							NewHitDataList.AddNoResize(new HitData
							{
								Pos = neighbor.Pos,
								Attacker = owner,
								Target = neighbor.Entity,
								DistanceSq = distancesq,
								CurrentDuration = bullet.DelayBetweenHits,
							});

						} 
						while (PartitionHashMap.TryGetNextValue(out neighbor, ref it));
					}
				}
				
				neighbors.Dispose();
			}

			private bool alreadyHit(Entity owner, Entity enemy)
			{
				if (HitDataHashMap.TryGetFirstValue(owner, out HitData hit, out var it))
				{
					do
					{
						if (enemy == hit.Target)
						{
							return true;
						}
					} 
					while (HitDataHashMap.TryGetNextValue(out hit, ref it));
				}
				
				return false;
			}
		}

	}
	
	[System.Serializable]
	public struct HitData 
	{
		public float2 Pos;
		public Entity Target;
		public Entity Attacker;
		public float DistanceSq;
		public float CurrentDuration;

		public override string ToString()
		{
			return $"{Attacker} \t-> {Target} \t| {CurrentDuration}";
		}
		
	}
}
