#define DEBUG_PATH
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Tertle.DestroyCleanup;

	public partial struct PathfindingSystem : ISystem
	{
		public float Spacing;
		public int SpacingInt;
		public int2 GridSize;
		
		public GridSingleton AreaPartitionSingleton;
		
		public const int MOVE_STRAIGHT_COST = 10;
		public const int MOVE_DIAGONAL_COST = 14;
		public const int FAILURE_INDEX = -9999;
		private NativeArray<int2> _check9;
		DynamicBuffer<SpawnDataBufferSingleton> _spawnDatas;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyIdComponent>();
			state.RequireForUpdate<DebugDisabledTag>();
			
			_check9 = new NativeArray<int2>(9, Allocator.Persistent);
			_check9[0]= new int2(0, 0);
			_check9[1]= new int2(0, 1);
			_check9[2]= new int2(1, 0);
			_check9[3]= new int2(0, -1);
			_check9[4]= new int2(-1, 0);
			_check9[5]= new int2(1, 1);
			_check9[6]= new int2(1, -1);
			_check9[7]= new int2(-1, 1);
			_check9[8]= new int2(-1, -1);
		}
		
		public void OnDestroy(ref SystemState state)
		{
			_check9.Dispose();
		}

		// [BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			SystemAPI.TryGetSingleton(out AreaPartitionSingleton);
			Spacing = AreaPartitionSingleton.Spacing;
			SpacingInt = (int)math.ceil(Spacing);
			GridSize = (int2)math.ceil(AreaPartitionSingleton.Size);
			_spawnDatas = SystemAPI.GetSingletonBuffer<SpawnDataBufferSingleton>();
			
			// TODO: create grid
			// calculate path
			foreach (var (data, agent, buffer, localTransform, owner) in SystemAPI.Query<
				RefRO<EnemyIdComponent>, RefRW<AgentComponent>, DynamicBuffer<AgentPathBuffer>, RefRO<LocalTransform>
				>().WithEntityAccess())
			{
				// if (agent.ValueRO.IsDoneCalculatePath) { continue;}
				// agent.ValueRW.IsDoneCalculatePath = true;
				
				NativeHashMap<int2, PathNode> pathNodeHashMap = new NativeHashMap<int2, PathNode>(GridSize.x * GridSize.y, Allocator.Temp);
				int2 startPos = (int2)math.ceil(localTransform.ValueRO.Position.xz);
				int2 endPos = (int2)math.ceil(agent.ValueRO.Destination);
				#if DEBUG_PATH
				// Debug.Log($"pathfind {getAreaPartition(startPos)} to {getAreaPartition(endPos)}");
				#endif
				for (int x = -GridSize.x; x < GridSize.x; x++)
				{
					for (int y = -GridSize.x; y < GridSize.y; y++)
					{
						int2 pos = new int2(x, y);
						
						
						
						PathNode p = new PathNode
						{
							Pos = pos,
							Index = getAreaPartition(pos),
							
							GCost = int.MaxValue,
							HCost = calculateDistanceCost(pos, endPos),
							
							IsWalkable = isWalkableHardcode(pos),
							ComeFromIndex = FAILURE_INDEX,
						};
						p.CalculateFCost();
						
						if (!p.IsWalkable)
						{
							var wall = state.EntityManager.Instantiate(_spawnDatas[1].Entity);	
							state.EntityManager.SetComponentData(wall, 
								LocalTransform.FromPosition(p.Pos.x - (Spacing * 0.5f), 0, p.Pos.y - (Spacing * 0.5f)));
						}
						// buffer.Add(new AgentPathBuffer
						// {
						// 	PathNode = p,
						// });
						
						pathNodeHashMap[p.Index] = p;
					}
				}
				
				int2 endNodeIndex = getAreaPartition(endPos);
				
				PathNode startNode = pathNodeHashMap[getAreaPartition(startPos)];
				startNode.GCost = 0;
				startNode.CalculateFCost();
				pathNodeHashMap[startNode.Index] = startNode;
				
				var openList = new NativeList<int2>(Allocator.Temp);
				var closedList = new NativeList<int2>(Allocator.Temp);
				
				openList.Add(startNode.Index);
				
				while (openList.Length > 0)
				{
					int2 currentNodeIndex = getLowestCostFNodeIndex(openList, pathNodeHashMap);
					PathNode currentNode = pathNodeHashMap[currentNodeIndex];
					
					if (math.all(currentNodeIndex == endNodeIndex))
					{
						// reach our destination
						break;
					}
					
					for (int i = 0; i < openList.Length; i++)
					{
						if (math.all(openList[i] == currentNodeIndex))
						{
							openList.RemoveAtSwapBack(i);
						}
					}
					
					closedList.Add(currentNodeIndex);
					
					for (int i = 0; i < _check9.Length; i++)
					{
						int2 neighborOffset = _check9[i];
						int2 neighborPos = new int2(currentNode.Pos + neighborOffset);
						
						if (!isPosInsideGrid(neighborPos))
						{
							continue;
						}
						
						int2 neighborIndex = getAreaPartition(neighborPos);
						
						if (closedList.Contains(neighborIndex))
						{
							continue;
						}
						
						PathNode neighborNode = pathNodeHashMap[neighborIndex];
						if (!neighborNode.IsWalkable)
						{
							continue;
						}
						
						int tentativeGCost = currentNode.GCost + calculateDistanceCost(currentNode.Pos, neighborPos);
						if (tentativeGCost < neighborNode.GCost)
						{
							neighborNode.ComeFromIndex = currentNodeIndex;
							neighborNode.GCost = tentativeGCost;
							neighborNode.CalculateFCost();
							
							pathNodeHashMap[neighborIndex] = neighborNode;
							
							if (!openList.Contains(neighborNode.Index))
							{
								openList.Add(neighborNode.Index);
							}
						}
					}
				}
				
				PathNode endNode = pathNodeHashMap[endNodeIndex];
				if (math.all(endNode.ComeFromIndex == FAILURE_INDEX))
				{
					
				}
				else
				{
					calculatePath(pathNodeHashMap, endNode, buffer);
					
				}
				
				pathNodeHashMap.Dispose();
				
			}
		}
		
		// TODO: create wall
		private bool isWalkableHardcode(int2 pos)
		{
			if (pos.y == -5)
			{
				if (pos.x > -6 && pos.x < 2)
				{
					return false;
				}
			}
			
			if (pos.x == -6)
			{
				if (pos.y > -6 && pos.y < 2)
				{
					return false;
				}
			}
			
			if (pos.y == 1)
			{
				if (pos.x > -6 && pos.x < 2)
				{
					return false;
				}
			}
			
			if (pos.x == 1)
			{
				if (pos.y > -3 && pos.y < 2)
				{
					return false;
				}
			}
			
			return true;
		}

		private void calculatePath(NativeHashMap<int2, PathNode> pathnodeArray, PathNode endNode, DynamicBuffer<AgentPathBuffer> buffer)
		{
			if (math.all(endNode.ComeFromIndex == FAILURE_INDEX))
			{
				return;
			}
			else
			{
				buffer.Add(new AgentPathBuffer
				{
					Value = (float2)endNode.Pos - (Spacing * 0.5f),	
				});
				
				PathNode currentNode = endNode;
				while (math.all(currentNode.ComeFromIndex != FAILURE_INDEX))
				{
					PathNode comeNode = pathnodeArray[currentNode.ComeFromIndex];
					int bufferIndex = buffer.Add(new AgentPathBuffer
					{
						Value = (float2)comeNode.Pos - (Spacing * 0.5f),
					});
					#if DEBUG_PATH
					if (buffer.Length >= 2)
					{
						Debug.DrawLine(
							new Vector3(buffer[bufferIndex-1].Value.x, 0, buffer[bufferIndex-1].Value.y), 
							new Vector3(buffer[bufferIndex].Value.x, 0, buffer[bufferIndex].Value.y),
							Color.cyan);
						
					}
					#endif
					currentNode = comeNode;
					
				}
			}
		}
		
		private bool isPosInsideGrid(int2 gridPosition)
		{
			return 
				gridPosition.x < GridSize.x &&
				gridPosition.x > -GridSize.x &&
				
				gridPosition.y < GridSize.y &&
				gridPosition.y > -GridSize.y;
			
		}
		
		private int calculateDistanceCost(int2 aPos, int2 bPos)
		{
			int xDistance = math.abs(aPos.x - bPos.x);
			int yDistance = math.abs(aPos.y - bPos.y);
			int remaining = math.abs(xDistance - yDistance);
			
			return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
		}
		
		private int2 getLowestCostFNodeIndex(NativeList<int2> openList, NativeHashMap<int2, PathNode> pathNodeArray)
		{
			PathNode lowestNode = pathNodeArray[openList[0]];
			for (int i = 0; i < openList.Length; i++)
			{
				PathNode testPathNode = pathNodeArray[openList[i]];
				if (testPathNode.FCost < lowestNode.FCost)
				{
					lowestNode = testPathNode;
				}
			}
			
			return lowestNode.Index;
		}
		// private int2 getIndex(int2 pos)
		// {
		// 	return pos.x + pos.y * GridSize.x;
		// }
		
		// todo: index
		private int2 getAreaPartition(int2 pos)
		{
			return pos; // new int2(pos.x / SpacingInt, pos.y / SpacingInt);
		}
		private int2 getAreaPartition(float2 pos)
		{
			return new int2((int)math.ceil(pos.x / Spacing), (int)math.ceil(pos.y / Spacing));
		}
	}
	
	[System.Serializable]
	public struct PathNode
	{
		public int2 Pos;
		
		public int2 Index;
		public int GCost;
		public int HCost;
		public int FCost;
		
		public bool IsWalkable;
		public int2 ComeFromIndex;
		
		public void CalculateFCost()
		{
			FCost = GCost + FCost;
		}
	}
	
	[BurstCompile]
	public partial struct PathfindingSystemJob : IJobEntity
	{
		// public float deltaTime;
		// public float multiplierDeltaTime;
		
		[BurstCompile]
		public void Execute(Entity owner, [ChunkIndexInQuery] int chunkIndex)
		{
			
		}
	}

