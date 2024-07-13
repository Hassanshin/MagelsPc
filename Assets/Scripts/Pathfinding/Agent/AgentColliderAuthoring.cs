using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class AgentColliderAuthoring : MonoBehaviour
	{
		public AgentColliderComponent Comp = new()
		{
			Radius = 1,
			RadiusInt = 1,
		};
	}

	public class AgentColliderAuthoringBaker : Baker<AgentColliderAuthoring>
	{
		public override void Bake(AgentColliderAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, authoring.Comp);
		}
	}
}
	
	[System.Serializable]
	public struct AgentColliderComponent : IComponentData
	{
		public float Radius;
		public int RadiusInt;
		public ENUM_COLLIDER_SHAPE Shape;
		public ENUM_COLLIDER_LAYER CollisionLayer;
		public ENUM_COLLIDER_LAYER CollideWith;
	}
	
