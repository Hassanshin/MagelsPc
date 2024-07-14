using Hash.HashMap;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

	public abstract class BaseWeapon : MonoBehaviour
	{
		[SerializeField]
		protected int _bulletId;
		[SerializeField]
		protected BulletComponent _bulletComp;
		
		[Header("State")]
		[SerializeField]
		protected int _currentAmmo;
		
		[SerializeField]
		protected int _maxAmmo;
		[SerializeField]
		protected float _currentReloadTime;
		[SerializeField]
		protected float _maxAmmoReloadTime;
		[Header("Data")]
		protected EntityManager _entityManager;
		protected DynamicBuffer<BulletDataBufferSingleton> _buffer;
		
		public bool AbleToFire => _currentAmmo > 0;
		[SerializeField]
		protected Vector3 _playerPos => this.transform.position;
		[SerializeField]
		protected Vector3 _playerDirection => this.transform.forward;
		
		[Header("VFX")]
		[SerializeField]
		private ParticleSystem _vfxHit;
		[SerializeField]
		private ParticleSystem _vfxKill;
		
		public abstract void Reload();
		public abstract void Shoot();
		public abstract void OnHit(HitData data);
		
		
		protected virtual void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_bulletComp.Weapon = this;
		}
		
		protected virtual void FixedUpdate()
		{
			
		}
		
		protected virtual Entity SpawnEntityBullet(LocalTransform localTransform)
		{
			_buffer = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(BulletDataBufferSingleton)}).GetSingletonBuffer<BulletDataBufferSingleton>();
			var spawned = _entityManager.Instantiate(_buffer[_bulletId].Entity);
			
			_entityManager.SetComponentData(spawned, localTransform);
			_entityManager.SetComponentData(spawned, new MoveForwardComponent(_bulletComp.Speed, 0));
			_entityManager.SetComponentData(spawned, _bulletComp);
			
			
			return spawned;
		}
		
		protected GameObject SpawnVfxHit(Vector3 pos, Quaternion rot)
		{
			return Instantiate(_vfxHit, pos, rot).gameObject;
		}
		
		protected GameObject SpawnVfxKill(Vector3 pos, Quaternion rot)
		{
			return Instantiate(_vfxKill, pos, rot).gameObject;
		}
	}
