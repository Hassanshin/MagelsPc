using Hash.HashMap;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Baker
{
	public class HitBufferSingletonAuthoring : MonoBehaviour
	{
		
	}

	public class HitBufferSingletonAuthoringBaker : Baker<HitBufferSingletonAuthoring>
	{
		public override void Bake(HitBufferSingletonAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			
			AddBuffer<HitBufferDataMono>(entity);
		}
	}
}

	[System.Serializable]
	public struct HitBufferDataMono : IBufferElementData
	{
		public UnityObjectRef<BaseWeapon> Weapon;
		public HitData Hit;
	}
