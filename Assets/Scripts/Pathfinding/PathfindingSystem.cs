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

namespace Hash.PathFinding
{
	
	public partial struct PathfindingSystem : ISystem
	{
		public float Spacing;
		public int SpacingInt;
		public int2 GridSize;
		
		private GridSingleton _gridSingleton;
		private DynamicBuffer<GridBuffer> _gridBuffers;
		public float DeltaTime;
		
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
			DeltaTime = SystemAPI.Time.DeltaTime;
			
			// var pathFindingJob = 
			new PathFindingSystemJob()
			{
				GridSingleton = _gridSingleton,
				GridBuffers = _gridBuffers,
				DeltaTime = DeltaTime,
				
			}.ScheduleParallel();
			// pathFindingJob.Complete();
		}
	}
	
	[BurstCompile]
	public partial struct PathFindingSystemJob : IJobEntity
	{
		[ReadOnly]
		public GridSingleton GridSingleton;
		[ReadOnly]
		public DynamicBuffer<GridBuffer> GridBuffers;
		[ReadOnly]
		public float DeltaTime;
		
		[BurstCompile]
		public void Execute(Entity owner, [ChunkIndexInQuery] int chunkIndex,
			in EnemyIdComponent data, ref AgentPathComponent agent, DynamicBuffer<AgentPathBuffer> buffer, in LocalTransform localTransform)
		{
			if (agent.CurrentUpdateFrequency < agent.MaxUpdateFrequency)
			{
				agent.CurrentUpdateFrequency += DeltaTime;
				return;
			}
			agent.CurrentUpdateFrequency = 0;
			
			float2 startPos = localTransform.Position.xz;
			float2 endPos = agent.Destination;
			NativeArray<GridBuffer> gridArray = GridBuffers.ToNativeArray(Allocator.Temp);
			
			if (!GridSingleton.IsOnValidGrid(endPos) || !GridSingleton.IsOnValidGrid(startPos))
			{
				if (buffer.Length > 0)
				{
					buffer.Clear();
				}
				
				gridArray.Dispose();
				return;
			}
			
			for (int i = 0; i < gridArray.Length; i++)
			{
				float2 pos = GridSingleton.GetPosFromId(i);
				
				GridBuffer p = gridArray[i]; 
				p.Value.HCost = calculateDistanceCost(pos, endPos);
				gridArray[i] = p;
			}
			
			int endNodeIndex = GridSingleton.GetIdFromPos(endPos);
			
			GridBuffer startNode = gridArray[data.PartitionId];
			startNode.Value.GCost = 0; 
			gridArray[startNode.Value.Index] = startNode;
			
			var openList = new NativeList<int>(Allocator.Temp);
			var closedList = new NativeList<int>(Allocator.Temp);
			
			openList.Add(startNode.Value.Index);
			
			while (openList.Length > 0)
			{
				int currentNodeIndex = getLowestCostFNodeIndex(openList, gridArray);
				GridBuffer currentNode = gridArray[currentNodeIndex];
				
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
				GridSingleton.GetNeighborId(ref neighborList, currentNodeIndex, 1);
				
				for (int i = 0; i < neighborList.Length; i++)
				{
					int neighborIndex = neighborList[i];
					float2 neighborPos = GridSingleton.GetPosFromId(neighborIndex);
					
					if (closedList.Contains(neighborIndex))
					{
						continue;
					}
					
					GridBuffer neighborNode = gridArray[neighborIndex];
					if (!neighborNode.Value.IsWalkable)
					{
						continue;
					}
					
					float tentativeGCost = currentNode.Value.GCost + calculateDistanceCost(currentNode.Value.Pos, neighborPos);
					if (tentativeGCost < neighborNode.Value.GCost)
					{
						neighborNode.Value.ComeFromIndex = currentNodeIndex;
						neighborNode.Value.GCost = tentativeGCost;
						
						gridArray[neighborIndex] = neighborNode;
						
						if (!openList.Contains(neighborNode.Value.Index))
						{
							openList.Add(neighborNode.Value.Index);
						}
					}
				}
				
				neighborList.Dispose();
			}
			
			PathNode endNode = gridArray[endNodeIndex].Value;
			if (endNode.ComeFromIndex == -1)
			{
				
			}
			else
			{
				buffer.Clear();
				buffer.Add(new AgentPathBuffer
				{
					Value = endPos,	
				});
				calculatePath(gridArray, endNode, buffer);
				buffer.ElementAt(buffer.Length -1 ).Value = startPos;
			}
			
			gridArray.Dispose();
		}
		
		private void calculatePath(NativeArray<GridBuffer> pathNodeArray, PathNode endNode, DynamicBuffer<AgentPathBuffer> buffer)
		{
			if (endNode.ComeFromIndex == -1)
			{
				return;
			}
			
			PathNode currentNode = endNode;
			while (currentNode.ComeFromIndex != -1)
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
		
		private float calculateDistanceCost(float2 aPos, float2 bPos)
		{
			float xDistance = math.abs(aPos.x - bPos.x);
			float yDistance = math.abs(aPos.y - bPos.y);
			float remaining = math.abs(xDistance - yDistance);
			
			return 14 * math.min(xDistance, yDistance) + 10 * remaining;
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
}

