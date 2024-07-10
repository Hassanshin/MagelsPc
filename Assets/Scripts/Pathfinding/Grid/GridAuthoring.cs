using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using System.Collections.Generic;

namespace Baker
{
	public class GridAuthoring : MonoBehaviour
	{
		public bool ShowGizmo;
		public float2 Size;
		public float2 Division => Size / Spacing;
		public Color Color = Color.white;
		
		public float Spacing = 10;
		public float2 corner => new float2(transform.position.x, transform.position.z) + (Size * 0.5f);
		public List<float2> Partitions = new List<float2>();
		
		#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if (!ShowGizmo) { return; }
			Partitions.Clear();
			
			// Draw a WIRE cube for all surface/border
			Gizmos.color = Color;
			Gizmos.DrawWireCube(transform.position, new Vector3(Size.x, 0, Size.y));
			
			// dont let 0 spacing
			if (Spacing <= 0.1f)
			{
				return;
			}
			
			// Draw a division
			int counter = 0;
			for (int i = 0; i < Division.x; i++)
			{
				for (int j = 0; j < Division.y; j++)
				{
					float midX = getMidX(i);
					float midY = getMidY(j);
					Gizmos.DrawWireCube(transform.position + new Vector3(midX, 0, midY), new Vector3(Spacing, 0, Spacing));
					
					float cornerX = midX - Spacing * 0.5f;
					float cornerY = midY + Spacing * 0.5f;
					Partitions.Add(new float2(midX, midY));
					
					counter++;
					
					int id = GetIdFromPos(new float2(midX, midY));
					UnityEditor.Handles.Label(transform.position + new Vector3(midX, 0, midY), $"{id}");
				}
			}
		}
		#endif
		
		public int GetIdFromPos(float2 pos)
		{
			// Translate pos to the grid origin
			pos += Size * 0.5f;

			// Convert the position to grid coordinates using integer arithmetic
			int xIndex = (int)(pos.x / Spacing);
			int yIndex = (int)(pos.y / Spacing);

			// Calculate the unique index
			int gridWidth = (int)(Size.x / Spacing);
			return yIndex * gridWidth + xIndex;
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
				Count = authoring.Partitions.Count,
				Size = authoring.Size,
				Width = (int)math.ceil(authoring.Size.x),
				MaxIndex = (int)math.ceil(authoring.Size.x * authoring.Size.y) - 1,
				HalfSize = authoring.Size * 0.5f,
				Origin = new float2(authoring.transform.position.x, authoring.transform.position.z),
			});
			
			// bakeBuffer(entity, authoring.Partitions, authoring.Spacing * 0.5f);
		}
	}
}

	public struct GridSingleton : IComponentData
	{
		public float Spacing;
		public int Count;
		public int MaxIndex;
		public int Width;
		public float2 Size;
		public float2 HalfSize;
		public float2 Origin;
		
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
						list.Add(neighborID);
					}
				}
			}
		}
		
		public readonly int GetIdFromPos(float2 pos)
		{
			// Translate pos to the grid origin
			pos += HalfSize;
			
			// Convert the position to grid coordinates using integer arithmetic
			int xIndex = (int)(pos.x / Spacing);
			int yIndex = (int)(pos.y / Spacing);

			// Calculate the unique index
			int gridWidth = (int)(Size.x / Spacing);
			
			// Debug.Log($"{pos} -> {yIndex * gridWidth + xIndex}");
			return yIndex * gridWidth + xIndex;
		}
	}
