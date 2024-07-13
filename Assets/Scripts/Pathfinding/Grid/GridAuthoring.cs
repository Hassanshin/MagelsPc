using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using System.Collections.Generic;


namespace Baker
{
	public class GridAuthoring : MonoBehaviour
	{
		public float2 Size;
		public float2 Division => Size / Spacing;
		public float Spacing = 10;
		public float2 corner => new float2(transform.position.x, transform.position.z) + (Size * 0.5f);
		public LayerMask ObstacleLayerMask;
		public float ObstacleCheckRadius = 0.5f;
		[Header("Debug")]
		public Color FineColor = Color.white;
		public Color BadColor = Color.red;
		public bool ShowGizmo;
		public bool ShowText;
		public float DistancePercentCheck = 0.5f;
		
		[Header("Result")]
		public PathNode[] Partitions;
		
		public void Start()
		{
			BakeWalkable();
		}
		
		[ContextMenu("BAKE")]
		public void BakeWalkable()
		{
			Partitions = new PathNode[Mathf.CeilToInt(Division.x * Division.y)];
			Debug.Log(Partitions);
			// dont let 0 spacing
			if (Spacing <= 0.1f)
			{
				return;
			}
			
			// Get the origin
			float2 origin = new float2(transform.position.x, transform.position.z);
			
			// Draw a division
			int counter = 0;
			for (int i = 0; i < Division.x; i++)
			{
				for (int j = 0; j < Division.y; j++)
				{
					float midX = getMidX(i);
					float midY = getMidY(j);
					
					float2 pos = new float2(midX, midY) + origin;
					int index = GetIdFromPos(pos);
					Partitions[index] = new PathNode
					{
						Pos = pos,
						IsWalkable = !Physics.CheckSphere(new Vector3(midX + origin.x, 0, midY + origin.y), ObstacleCheckRadius, ObstacleLayerMask),
						Index = index,
						ComeFromIndex = -1,
						
						GCost = int.MaxValue,
					};
					Debug.Log(Partitions[counter]);
					counter++;
				}
			}
		}
		
		public void OnDrawGizmos()
		{
			if (!ShowGizmo) { return; }
			
			for (int i = 0; i < Partitions.Length; i++)
			{
				Gizmos.color = Partitions[i].IsWalkable ? FineColor : BadColor;
				
				Gizmos.DrawWireCube((Vector3)Partitions[i].GetFloat3, new Vector3(Spacing-0.1f, 0, Spacing-0.1f));
				#if UNITY_EDITOR
				if (ShowText)
				{
					int id = GetIdFromPos(Partitions[i].Pos);
					float2 pos = GetPosFromId(id);
					UnityEditor.Handles.Label((Vector3)Partitions[i].GetFloat3, $"{id}\n{pos.x},{pos.y}");
				}
				#endif
			}
		}
		
		public int GetIdFromPos(float2 pos)
		{
			// Translate pos to the grid origin
			float2 origin = new float2(transform.position.x, transform.position.z);
			pos -= origin;
			pos += Size * 0.5f;
			
			// Convert the position to grid coordinates using integer arithmetic
			int xIndex = (int)(pos.x / Spacing);
			int yIndex = (int)(pos.y / Spacing);

			// Calculate the unique index
			int gridWidth = (int)(Size.x / Spacing);
			
			// Debug.Log($"{pos} -> {yIndex * gridWidth + xIndex}");
			return yIndex * gridWidth + xIndex;
		}
		
		public float2 GetPosFromId(int index)
		{
			int gridWidth = (int)(Size.x / Spacing);

			// Calculate the x and y indices from the given index
			int yIndex = index / gridWidth;
			int xIndex = index % gridWidth;

			// Convert the grid indices back to the position
			float xPos = xIndex * Spacing - Size.x * 0.5f;
			float yPos = yIndex * Spacing - Size.y * 0.5f;

			return new float2(xPos, yPos);
		}

		private float getMidX(int i)
		{
			return (Spacing * i) + Spacing * 0.5f + -Spacing * (Division.x * 0.5f);
		}

		private float getMidY(int j)
		{
			return (Spacing * j) + Spacing * 0.5f + -Spacing * (Division.y * 0.5f);
		}
	}

	public class GridAuthoringBaker : Baker<GridAuthoring>
	{
		public override void Bake(GridAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new GridSingleton
			{
				// Value = authoring.Value
				Spacing = authoring.Spacing,
				Count = authoring.Partitions.Length,
				Size = authoring.Size,
				Width = (int)math.ceil(authoring.Size.x),
				MaxIndex = (int)math.ceil(authoring.Size.x * authoring.Size.y) - 1,
				HalfSize = authoring.Size * 0.5f,
				DistanceUpdateCheck = math.pow(authoring.Size * authoring.DistancePercentCheck, 2),
				Origin = new float2(authoring.transform.position.x, authoring.transform.position.z),
				
				ObstacleCheckRadius = authoring.ObstacleCheckRadius,
				ObstacleLayerMask = authoring.ObstacleLayerMask,
			});
			
			AddComponent<GridBuffer>(entity);
			var buffer = SetBuffer<GridBuffer>(entity);
			
			foreach (var item in authoring.Partitions)
			{
				buffer.Add(new GridBuffer
				{
					Value = item,
				});
			}
		}
	}
}
	
	public struct GridBuffer : IBufferElementData
	{
		public PathNode Value;
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
			return IsWalkable ?  
				$"<color=white>{Pos}. {Index} W: {IsWalkable}</color>":
				$"<color=red>{Pos}. {Index} W: {IsWalkable}</color>"
				;
		}
	
	}
	
	public struct GridSingleton : IComponentData
	{
		public float Spacing;
		public float2 Division => Size / Spacing;
		
		public int Count;
		public int MaxIndex;
		public int Width;
		public float2 Size;
		public float2 HalfSize;
		public float2 DistanceUpdateCheck;
		public float2 Origin;
		public float ObstacleCheckRadius;
		public LayerMask ObstacleLayerMask;
		
		public readonly bool IsOnValidGrid(float2 pos)
		{
			return 
				pos.x < Origin.x + HalfSize.x &&
				pos.x >= Origin.x - HalfSize.x &&
				pos.y < Origin.y + HalfSize.y &&
				pos.y >= Origin.y - HalfSize.y;
		}
		
		public readonly bool IsOnValidGrid(int id)
		{
			return id > 0 && id < Count;
		}
		
		public readonly int CalculateNeighborCount(int radius)
		{
			// Calculate the total number of points in the given radius
			return 1 + 8 * radius * (radius + 1) / 2;
		}
		
		public readonly void GetNeighborId(ref NativeList<int> list, int id, int radius)
		{
			// Determine the (x, y) position of the given id
			int width = (int)(Width / Spacing);
			int2 pos = new int2(id % width, id / width);

			// Loop through the neighbors within the given radius
			for (int dy = -radius; dy <= radius; dy++)
			{
				for (int dx = -radius; dx <= radius; dx++)
				{
					int2 neighborPos = pos + new int2(dx, dy);

					// Check if the neighbor position is within the grid bounds
					if (neighborPos.x >= 0 && neighborPos.x < width && neighborPos.y >= 0 && neighborPos.y < width)
					{
						int neighborID = neighborPos.y * width + neighborPos.x;
						if (neighborID != id)
						{
							list.AddNoResize(neighborID);
						}
					}
				}
			}
		}
		
		public readonly int GetIdFromPos(float2 pos)
		{
			// Translate pos to the grid origin
			pos += -Origin + HalfSize;
			
			// Convert the position to grid coordinates using integer arithmetic
			int xIndex = (int)(pos.x / Spacing);
			int yIndex = (int)(pos.y / Spacing);

			// Calculate the unique index
			int gridWidth = (int)(Size.x / Spacing);
			
			// Debug.Log($"{pos} -> {yIndex * gridWidth + xIndex}");
			return yIndex * gridWidth + xIndex;
		}
		
		public readonly float2 GetPosFromId(int index)
		{
			int gridWidth = (int)(Size.x / Spacing);

			// Calculate the x and y indices from the given index
			int yIndex = index / gridWidth;
			int xIndex = index % gridWidth;

			// Convert the grid indices back to the position
			float xPos = xIndex * Spacing - HalfSize.x;
			float yPos = yIndex * Spacing - HalfSize.y;

			return new float2(xPos, yPos);
		}
		
		public float GetMidX(int i)
		{
			return (Spacing * i) + Spacing * 0.5f + -Spacing * (Division.x * 0.5f);
		}

		public float GetMidY(int j)
		{
			return (Spacing * j) + Spacing * 0.5f + -Spacing * (Division.y * 0.5f);
		}
	}
