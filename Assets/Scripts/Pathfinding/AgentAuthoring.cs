using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentAuthoring : MonoBehaviour
	{
		public float Value;
		public float2 Destination;
	}

	public class AgentAuthoringBaker : Baker<AgentAuthoring>
	{
		public override void Bake(AgentAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
			
			AddComponent(entity, new AgentComponent
			{
				Value = authoring.Value,
				Destination = authoring.Destination,
			});
			
			AddBuffer<AgentPathBuffer>(entity);
		}
	}
}

	public struct AgentComponent : IComponentData
	{
		public float Value;
		public float2 Destination;
		public bool IsDoneCalculatePath;
	}
	
	public struct AgentPathBuffer : IBufferElementData
	{
		public float2 Value;
	}
