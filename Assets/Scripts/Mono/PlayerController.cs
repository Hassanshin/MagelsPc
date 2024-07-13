using System;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
	{
		private EntityManager _entityManager;
		private Entity _entity;
		[SerializeField]
		private Rigidbody _rigidbody;
		[SerializeField]
		private float _speed = 3;
		[SerializeField]
		private Vector3 _direction;
		[SerializeField]
		private float _velocity;
		[SerializeField]
		private Vector3 _input;
		private Camera _cam;
		private bool _isWalking;
		public bool IsWalking => _isWalking;
		
		public List<BaseWeapon> Weapons = new List<BaseWeapon>();
		private bool _isReady;
		private bool _isReloading;
		public void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_cam = Camera.main;
		}
		
		public void Update()
		{
			_isReady = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(PlayerTag)}).TryGetSingletonEntity<PlayerTag>(out _entity);
			if (!_isReady) { return; }
			
			if (Input.GetButtonDown("Fire1"))
			{
				shoot();
			}
			
			if (Input.GetKeyDown(KeyCode.R))
			{
				reload();
			}
		}

		

		public void FixedUpdate()
		{
			if (!_isReady) {return;}
			float hor = Input.GetAxis("Horizontal");
			float ver = Input.GetAxis("Vertical");
			
			_input.x = hor;
			_input.z = ver;
			
			_direction = new Vector3(hor, 0, ver);
			
			_rigidbody.velocity = _speed * Time.fixedDeltaTime * _direction.normalized;
			_isWalking = _rigidbody.velocity != Vector3.zero;
			if (!_isWalking)
			{
				_velocity = 0;
				
			}
			else
			{
				_velocity = _rigidbody.velocity.sqrMagnitude;
				transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
				
				_entityManager.SetComponentData(_entity, new LocalTransform
				{
					Position = this.transform.position,
					Rotation = this.transform.rotation,
					Scale = this.transform.localScale.y,
				});
			}
		}

		private void shoot()
		{
			for (int i = 0; i < Weapons.Count; i++)
			{
				Weapons[i].Shoot();
				
			}
		}
		
		private void reload()
		{
			for (int i = 0; i < Weapons.Count; i++)
			{
				Weapons[i].Reload();
			}
		}
	}
