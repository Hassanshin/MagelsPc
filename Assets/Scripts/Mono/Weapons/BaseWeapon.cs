using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

	public abstract class BaseWeapon : MonoBehaviour
	{
		[SerializeField]
		protected int _bulletId;
		[SerializeField]
		protected float _bulletSpeed;
		
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
		public abstract void Reload();
		
		public abstract void Shoot();
		
		protected virtual void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}
		
		protected virtual void FixedUpdate()
		{
			
		}
		
		protected virtual Entity SpawnEntityBullet(LocalTransform localTransform)
		{
			_buffer = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(BulletDataBufferSingleton)}).GetSingletonBuffer<BulletDataBufferSingleton>();
			var spawned = _entityManager.Instantiate(_buffer[_bulletId].Entity);
			_entityManager.SetComponentData(spawned, localTransform);
			return spawned;
		}
	}
