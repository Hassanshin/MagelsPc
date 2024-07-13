using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentMoveAuthoring : MonoBehaviour
	{
		public float Speed;
	}

	public class AgentMoveAuthoringBaker : Baker<AgentMoveAuthoring>
	{
		public override void Bake(AgentMoveAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
			
			AddComponent(entity, new AgentMoveComponent
			{
				Speed = authoring.Speed
			});
		}
	}
}

	public struct AgentMoveComponent : IComponentData
	{
		public float Speed;
		public float RotationSpeed;
		public float DistanceSqLeft;
		public bool IsStopped;
		public float3 Direction;
	}
