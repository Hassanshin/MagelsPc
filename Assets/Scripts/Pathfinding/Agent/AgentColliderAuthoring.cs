using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentColliderAuthoring : MonoBehaviour
	{
		public float Radius;
	}

	public class AgentColliderAuthoringBaker : Baker<AgentColliderAuthoring>
	{
		public override void Bake(AgentColliderAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new AgentColliderComponent
			{
				Radius = authoring.Radius,
				RadiusInt = Mathf.CeilToInt(authoring.Radius),
			});
		}
	}
}

	public struct AgentColliderComponent : IComponentData
	{
		public float Radius;
		public int RadiusInt;
	}