using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentPathAuthoring : MonoBehaviour
	{
		public float2 Destination;
	}

	public class AgentPathAuthoringBaker : Baker<AgentPathAuthoring>
	{
		public override void Bake(AgentPathAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
			
			AddComponent(entity, new AgentPathComponent
			{
				Destination = authoring.Destination,
			});
			
			AddBuffer<AgentPathBuffer>(entity);
		}
	}
}

	public struct AgentPathComponent : IComponentData
	{
		public float2 Destination;
		public bool IsDoneCalculatePath;
	}
	
	public struct AgentPathBuffer : IBufferElementData
	{
		public float2 Value;
	}
