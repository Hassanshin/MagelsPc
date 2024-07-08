using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

	public class AreaPartitionAuthoring : MonoBehaviour
	{
		public float2 Size;
		public float2 Division => Size / Spacing;
		public Color Color = Color.white;
		
		public float Spacing = 10;
		public float2 corner => new float2(transform.position.x, transform.position.z) + (Size * 0.5f);
		public List<float2> Partitions = new List<float2>();
		
		public void OnDrawGizmosSelected()
		{
			Partitions.Clear();
			
			// Draw a WIRE cube for all surface/border
			Gizmos.color = Color;
			Gizmos.DrawWireCube(transform.position, new Vector3(Size.x, 0, Size.y));
			
			// dont let 0 spacing
			if (Spacing <= 0)
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
					
					int2 id = getAreaPartition(new float2(midX, midY));
					UnityEditor.Handles.Label(transform.position + new Vector3(midX, 0, midY), $"{id.x},{id.y}");
				}
			}
		}
		
		private int2 getAreaPartition(float2 pos)
		{
			float spacing = Spacing;
			
			return new int2((int)math.ceil(pos.x / spacing), (int)math.ceil(pos.y / spacing));
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

	public class AreaPartitionAuthoringBaker : Baker<AreaPartitionAuthoring>
	{
		public override void Bake(AreaPartitionAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new AreaPartitionSingleton
			{
				// Value = authoring.Value
				Spacing = authoring.Spacing,
				Count = authoring.Partitions.Count,
				FullAreaSize = authoring.Size,
			});
			
			// bakeBuffer(entity, authoring.Partitions, authoring.Spacing * 0.5f);
		}
		
		// private void bakeBuffer(Entity entity, List<float2> list, float size)
		// {
		// 	DynamicBuffer<AreaPartitionBuffer> buffer = AddBuffer<AreaPartitionBuffer>(entity);
		// 	buffer.Length = list.Count;
			
		// 	for (int i = 0; i < list.Count; i++)
		// 	{
		// 		buffer[i] = new AreaPartitionBuffer 
		// 		{ 
		// 			Min = list[i].y - size, 
		// 			Max = list[i].x + size,
		// 		};	
		// 	}
		// }
	}

	public struct AreaPartitionSingleton : IComponentData
	{
		public float Spacing;
		public int Count;
		public float2 FullAreaSize;
	}
	
	public struct AreaPartitionBuffer : IBufferElementData
	{
		public float2 Min;
		public float2 Max;
	}
	
	// [System.Serializable]
	// public struct AreaPartitionData
	// {
	// 	public float3 Mid;
	// 	public float Area;
	// }
