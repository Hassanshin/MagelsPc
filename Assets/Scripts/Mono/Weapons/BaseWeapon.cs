using Hash.HashMap;
using Tertle.DestroyCleanup;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

	public abstract class BaseWeapon : MonoBehaviour
	{
		[SerializeField]
		protected int _bulletId;
		[SerializeField]
		public BulletComponent BulletComp;
		[SerializeField]
		public int TotalShot = 10;
		
		[SerializeField]
		public float MaxAngle = 10;
		[SerializeField]
		public float BulletDuration = 1;
		
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
		public bool IsReloading;
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
		public abstract bool Shoot();
		public abstract void OnHit(HitData data);
		
		
		protected virtual void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			BulletComp.Weapon = this;
		}
		
		protected virtual void FixedUpdate()
		{
			
		}
		
		protected virtual Entity SpawnEntityBullet(LocalTransform localTransform)
		{
			_buffer = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(BulletDataBufferSingleton)}).GetSingletonBuffer<BulletDataBufferSingleton>();
			var spawned = _entityManager.Instantiate(_buffer[_bulletId].Entity);
			
			_entityManager.SetComponentData(spawned, localTransform);
			_entityManager.SetComponentData(spawned, new MoveForwardComponent(BulletComp.Speed, 0));
			_entityManager.SetComponentData(spawned, BulletComp);
			_entityManager.SetComponentData(spawned, new DestroyByDurationComponent
			{
				Duration = BulletDuration,
				MaxDuration = BulletDuration,
			});
			
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
