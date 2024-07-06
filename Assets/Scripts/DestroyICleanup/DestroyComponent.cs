using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

namespace Tertle.DestroyCleanup
{
	public class DestroyByDurationAuthoring : MonoBehaviour
	{
		public float Duration = 1f;
		
		public class DestroyByDurationBaker : Baker<DestroyByDurationAuthoring>
		{
			public override void Bake(DestroyByDurationAuthoring authoring)
			{
				Entity entity = GetEntity(authoring, TransformUsageFlags.None);
				
				AddComponent(entity, new DestroyByDurationComponent
				{
					Duration = authoring.Duration,
					MaxDuration = authoring.Duration,
				});
				
				AddComponent<DestroyTag>(entity);
				SetComponentEnabled<DestroyTag>(entity, false);
			}
		} 
	}
	
	/// <summary> Unified destroy component allowing entities to all pass through a singular cleanup group. </summary>
	[ChangeFilterTracking]
	public struct DestroyTag : IComponentData, IEnableableComponent
	{
		
	}
	
	public struct DestroyByDurationComponent : IComponentData, IEnableableComponent
	{
		public float Duration;
		public float MaxDuration;
	}
}

public class ChangeFilterTrackingAttribute : Attribute
{
}
