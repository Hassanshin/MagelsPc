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
		private DynamicBuffer<GridBuffer> _gridBuffers;
		
		public const int MOVE_STRAIGHT_COST = 10;
		public const int MOVE_DIAGONAL_COST = 14;
		public const int FAILURE_INDEX = -1;
		
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyIdComponent>();
			state.RequireForUpdate<GridSingleton>();
			
		}
		
		public void OnDestroy(ref SystemState state)
		{
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			SystemAPI.TryGetSingleton(out _gridSingleton);
			Spacing = _gridSingleton.Spacing;
			SpacingInt = (int)math.ceil(Spacing);
			GridSize = (int2)math.ceil(_gridSingleton.Size);
			_gridBuffers = SystemAPI.GetSingletonBuffer<GridBuffer>();
			
			// calculate path
			foreach (var (data, agent, buffer, localTransform, owner) in SystemAPI.Query<
				RefRO<EnemyIdComponent>, RefRW<AgentComponent>, DynamicBuffer<AgentPathBuffer>, RefRO<LocalTransform>
				>().WithEntityAccess())
			{
				// if (agent.ValueRO.IsDoneCalculatePath) { continue;}
				// agent.ValueRW.IsDoneCalculatePath = true;
				
				
				NativeArray<GridBuffer> pathNodeList = _gridBuffers.ToNativeArray(Allocator.Temp);
				float2 startPos = _gridSingleton.GetPosFromId(data.ValueRO.PartitionId);
				float2 endPos = agent.ValueRO.Destination;
				
				if (!_gridSingleton.IsOnValidGrid(endPos) || !_gridSingleton.IsOnValidGrid(startPos))
				{
					continue;
				}
				
				for (int i = 0; i < pathNodeList.Length; i++)
				{
					float2 pos = _gridSingleton.GetPosFromId(i);
					
					GridBuffer p = pathNodeList[i]; 
					p.Value.HCost = calculateDistanceCost(pos, endPos);
					pathNodeList[i] = p;
				}
				
				int endNodeIndex = _gridSingleton.GetIdFromPos(endPos);
				
				GridBuffer startNode = pathNodeList[data.ValueRO.PartitionId];
				startNode.Value.GCost = 0; 
				pathNodeList[startNode.Value.Index] = startNode;
				
				var openList = new NativeList<int>(Allocator.Temp);
				var closedList = new NativeList<int>(Allocator.Temp);
				
				openList.Add(startNode.Value.Index);
				
				while (openList.Length > 0)
				{
					int currentNodeIndex = getLowestCostFNodeIndex(openList, pathNodeList);
					GridBuffer currentNode = pathNodeList[currentNodeIndex];
					
					#if DEBUG_PATH
					// UnityEngine.Debug.Log($"pathfind {currentNodeIndex}/{openList.Length} ");
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
					
					NativeList<int> neighborList = new(8, Allocator.Temp);
					_gridSingleton.GetNeighborId(ref neighborList, currentNodeIndex, 1);
					
					for (int i = 0; i < neighborList.Length; i++)
					{
						int neighborIndex = neighborList[i];
						float2 neighborPos = _gridSingleton.GetPosFromId(neighborIndex);
						
						if (closedList.Contains(neighborIndex))
						{
							continue;
						}
						
						GridBuffer neighborNode = pathNodeList[neighborIndex];
						if (!neighborNode.Value.IsWalkable)
						{
							continue;
						}
						
						float tentativeGCost = currentNode.Value.GCost + calculateDistanceCost(currentNode.Value.Pos, neighborPos);
						if (tentativeGCost < neighborNode.Value.GCost)
						{
							neighborNode.Value.ComeFromIndex = currentNodeIndex;
							neighborNode.Value.GCost = tentativeGCost;
							
							pathNodeList[neighborIndex] = neighborNode;
							
							if (!openList.Contains(neighborNode.Value.Index))
							{
								openList.Add(neighborNode.Value.Index);
							}
						}
					}
					
					neighborList.Dispose();
				}
				
				PathNode endNode = pathNodeList[endNodeIndex].Value;
				if (endNode.ComeFromIndex == FAILURE_INDEX)
				{
					
				}
				else
				{
					calculatePath(pathNodeList, endNode, buffer);
				}
				
				pathNodeList.Dispose();
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

		private void calculatePath(NativeArray<GridBuffer> pathNodeArray, PathNode endNode, DynamicBuffer<AgentPathBuffer> buffer)
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
					Value = endNode.Pos,	
				});
				
				PathNode currentNode = endNode;
				while (currentNode.ComeFromIndex != FAILURE_INDEX)
				{
					PathNode comeNode = pathNodeArray[currentNode.ComeFromIndex].Value;
					int bufferIndex = buffer.Add(new AgentPathBuffer
					{
						Value = comeNode.Pos,
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
		
		private int getLowestCostFNodeIndex(NativeList<int> openList, NativeArray<GridBuffer> pathNodeArray)
		{
			int lowestIndex = openList[0];
			PathNode lowestNode = pathNodeArray[lowestIndex].Value;

			for (int i = 1; i < openList.Length; i++)
			{
				PathNode testPathNode = pathNodeArray[openList[i]].Value;
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
		public readonly float3 GetFloat3 => new(Pos.x, 0, Pos.y);
		public int Index;
		public float GCost;
		public float HCost;
		public float FCost => GCost + HCost;
		
		public bool IsWalkable;
		public int ComeFromIndex;

		public override string ToString()
		{
			return Pos.ToString() + "." + Index + " W:" + IsWalkable;
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

