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
using System.Diagnostics;

public partial struct PathfindingSystem : ISystem
	{
		public float Spacing;
		public int SpacingInt;
		public int2 GridSize;
		
		private GridSingleton _gridSingleton;
		
		public const int MOVE_STRAIGHT_COST = 10;
		public const int MOVE_DIAGONAL_COST = 14;
		public const int FAILURE_INDEX = -1;
		
		DynamicBuffer<SpawnDataBufferSingleton> _spawnDatas;
		private bool _isSpawnedWall;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyIdComponent>();
			state.RequireForUpdate<GridSingleton>();
			
		}
		
		public void OnDestroy(ref SystemState state)
		{
		}

		// [BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			SystemAPI.TryGetSingleton(out _gridSingleton);
			Spacing = _gridSingleton.Spacing;
			SpacingInt = (int)math.ceil(Spacing);
			GridSize = (int2)math.ceil(_gridSingleton.Size);
			_spawnDatas = SystemAPI.GetSingletonBuffer<SpawnDataBufferSingleton>();
			
			// calculate path
			foreach (var (data, agent, buffer, localTransform, owner) in SystemAPI.Query<
				RefRO<EnemyIdComponent>, RefRW<AgentComponent>, DynamicBuffer<AgentPathBuffer>, RefRO<LocalTransform>
				>().WithEntityAccess())
			{
				// if (agent.ValueRO.IsDoneCalculatePath) { continue;}
				// agent.ValueRW.IsDoneCalculatePath = true;
				
				
				NativeList<PathNode> pathNodeList = new (_gridSingleton.Count, Allocator.Temp);
				float2 startPos = _gridSingleton.GetPosFromId(data.ValueRO.PartitionId);
				float2 endPos = agent.ValueRO.Destination;
				
				if (!_gridSingleton.IsOnValidGrid(endPos))
				{
					continue;
				}
				
				for (int i = 0; i < _gridSingleton.Count; i++)
				{
					float2 pos = _gridSingleton.GetPosFromId(i);
					PathNode p = new()
					{
						Pos = pos,
						Index = _gridSingleton.GetIdFromPos(pos),
						
						GCost = int.MaxValue,
						HCost = calculateDistanceCost(pos, endPos),
						
						IsWalkable = isWalkableHardcode(i),
						ComeFromIndex = FAILURE_INDEX,
					};
					
					if (!_isSpawnedWall && !p.IsWalkable)
					{
						var wall = state.EntityManager.Instantiate(_spawnDatas[1].Entity);	
						state.EntityManager.SetComponentData(wall, 
							LocalTransform.FromPosition(p.Pos.x + (Spacing * 0.5f), 0, p.Pos.y + (Spacing * 0.5f)));
							
					}
					
					pathNodeList.AddNoResize(p);
				}
				// Debug.Log("" + pathNodeList.Length);
				
				
				int endNodeIndex = _gridSingleton.GetIdFromPos(endPos);
				
				PathNode startNode = pathNodeList[data.ValueRO.PartitionId];
				#if DEBUG_PATH
				// Debug.Log($"pathfind {startNode} to {endPos}.{endNodeIndex}");
				#endif
				startNode.GCost = 0; 
				pathNodeList[startNode.Index] = startNode;
				
				var openList = new NativeList<int>(Allocator.Temp);
				var closedList = new NativeList<int>(Allocator.Temp);
				
				openList.Add(startNode.Index);
				
				while (openList.Length > 0)
				{
					int currentNodeIndex = getLowestCostFNodeIndex(openList, pathNodeList);
					PathNode currentNode = pathNodeList[currentNodeIndex];
					
					#if DEBUG_PATH
					// Debug.Log($"pathfind {currentNodeIndex}/{openList.Length} ");
					#endif
				
					if (currentNodeIndex == endNodeIndex)
					{
						// reach our destination
						break;
					}
					
					for (int i = 0; i < openList.Length; i++)
					{
						if (openList[i] == currentNodeIndex)
						{
							openList.RemoveAtSwapBack(i);
						}
					}
					
					closedList.Add(currentNodeIndex);
					
					NativeList<int> neighborList = new(9, Allocator.Temp);
					_gridSingleton.GetNeighborId(ref neighborList, currentNodeIndex, 1);
					
					for (int i = 0; i < neighborList.Length; i++)
					{
						int neighborIndex = neighborList[i];
						float2 neighborPos = _gridSingleton.GetPosFromId(neighborIndex);
						
						if (closedList.Contains(neighborIndex))
						{
							continue;
						}
						
						PathNode neighborNode = pathNodeList[neighborIndex];
						if (!neighborNode.IsWalkable)
						{
							continue;
						}
						
						float tentativeGCost = currentNode.GCost + calculateDistanceCost(currentNode.Pos, neighborPos);
						if (tentativeGCost < neighborNode.GCost)
						{
							neighborNode.ComeFromIndex = currentNodeIndex;
							neighborNode.GCost = tentativeGCost;
							
							pathNodeList[neighborIndex] = neighborNode;
							
							if (!openList.Contains(neighborNode.Index))
							{
								openList.Add(neighborNode.Index);
							}
						}
					}
					
					neighborList.Dispose();
				}
				
				PathNode endNode = pathNodeList[endNodeIndex];
				if (endNode.ComeFromIndex == FAILURE_INDEX)
				{
					
				}
				else
				{
					calculatePath(pathNodeList, endNode, buffer);
					
				}
				
				pathNodeList.Dispose();
				_isSpawnedWall = true;
			}
		}
		
		// TODO: create wall
		private bool isWalkableHardcode(int index)
		{
			if (index > 31 && index < 37)
			{
				return false;
			}
			
			
			return true;
		}

		private void calculatePath(NativeList<PathNode> pathnodeArray, PathNode endNode, DynamicBuffer<AgentPathBuffer> buffer)
		{
			if (endNode.ComeFromIndex == FAILURE_INDEX)
			{
				return;
			}
			else
			{
				buffer.Clear();
				buffer.Add(new AgentPathBuffer
				{
					Value = (float2)endNode.Pos + (Spacing * 0.5f),	
				});
				
				PathNode currentNode = endNode;
				while (currentNode.ComeFromIndex != FAILURE_INDEX)
				{
					PathNode comeNode = pathnodeArray[currentNode.ComeFromIndex];
					int bufferIndex = buffer.Add(new AgentPathBuffer
					{
						Value = (float2)comeNode.Pos + (Spacing * 0.5f),
					});
					#if DEBUG_PATH
					if (buffer.Length >= 2)
					{
						UnityEngine.Debug.DrawLine(
							new Vector3(buffer[bufferIndex-1].Value.x, 0, buffer[bufferIndex-1].Value.y), 
							new Vector3(buffer[bufferIndex].Value.x, 0, buffer[bufferIndex].Value.y),
							Color.cyan);
						
					}
					#endif
					currentNode = comeNode;
					
				}
			}
		}
		private float calculateDistanceCost(float2 aPos, float2 bPos)
		{
			float xDistance = math.abs(aPos.x - bPos.x);
			float yDistance = math.abs(aPos.y - bPos.y);
			float remaining = math.abs(xDistance - yDistance);
			
			return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
		}
		
		private int getLowestCostFNodeIndex(NativeList<int> openList, NativeList<PathNode> pathNodeArray)
		{
			int lowestIndex = openList[0];
			PathNode lowestNode = pathNodeArray[lowestIndex];

			for (int i = 1; i < openList.Length; i++)
			{
				PathNode testPathNode = pathNodeArray[openList[i]];
				if (testPathNode.FCost < lowestNode.FCost)
				{
					lowestNode = testPathNode;
					lowestIndex = openList[i];
				}
			}

			return lowestIndex;
		}
		
	}
	
	[System.Serializable]
	public struct PathNode
	{
		public float2 Pos;
		
		public int Index;
		public float GCost;
		public float HCost;
		public float FCost => GCost + HCost;
		
		public bool IsWalkable;
		public int ComeFromIndex;

		public override string ToString()
		{
			return Pos.ToString() + "." + Index;
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

