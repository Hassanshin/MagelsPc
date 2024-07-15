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
			
			AddBuffer<PlayerBulletHitBufferToMono>(entity);
			AddBuffer<PlayerGettingHitBufferToMono>(entity);
		}
	}
}

	[System.Serializable]
	public struct PlayerBulletHitBufferToMono : IBufferElementData
	{
		public UnityObjectRef<BaseWeapon> Weapon;
		public HitData Hit;
	}
	
	[System.Serializable]
	public struct PlayerGettingHitBufferToMono : IBufferElementData
	{
		public ENUM_COLLIDER_LAYER Layer;
		public PowerUpsComponent PowerUps;
		public float3 Pos;
	}
