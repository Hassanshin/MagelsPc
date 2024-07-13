using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class MoveForwardAuthoring : MonoBehaviour
	{
		public float Speed;
		public float AccelSpeed;
	}

	public class MoveForwardAuthoringBaker : Baker<MoveForwardAuthoring>
	{
		public override void Bake(MoveForwardAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddComponent(entity, new MoveForwardComponent(authoring.Speed, authoring.AccelSpeed));
		}
	}
}

	public struct MoveForwardComponent : IComponentData
	{
		public float Speed;
		public float AccelSpeed;
		public float MaxSpeed;
		public float MaxAccelSpeed;
		
		public MoveForwardComponent(float speed, float accelSpeed)
		{
			Speed = speed;
			AccelSpeed = accelSpeed;
			
			MaxSpeed = speed;
			MaxAccelSpeed = accelSpeed;
		}
	}
