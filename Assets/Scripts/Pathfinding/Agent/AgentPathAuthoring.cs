using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentPathAuthoring : MonoBehaviour
	{
		public float2 Destination;
		public float UpdateFrequency;
	}

	public class AgentPathAuthoringBaker : Baker<AgentPathAuthoring>
	{
		public override void Bake(AgentPathAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
			
			AddComponent(entity, new AgentPathComponent(authoring.Destination, authoring.UpdateFrequency));
			
			AddBuffer<AgentPathBuffer>(entity);
		}
	}
}

	public struct AgentPathComponent : IComponentData
	{
		public float2 Destination;
		public bool IsDoneCalculatePath;
		public readonly float MaxUpdateFrequency;
		public float CurrentUpdateFrequency;
		
		public AgentPathComponent(float2 destination, float updateFrequency)
		{
			Destination = destination;
			IsDoneCalculatePath = false;
			MaxUpdateFrequency = updateFrequency;
			CurrentUpdateFrequency = updateFrequency;
		}
	}
	
	public struct AgentPathBuffer : IBufferElementData
	{
		public float2 Value;
	}
