using System;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

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
		
		public void Start()
		{
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_cam = Camera.main;
		}
		
		public void Update()
		{
			if (_entityManager.CreateEntityQuery(new ComponentType[] { typeof(PlayerTag)}).TryGetSingletonEntity<PlayerTag>(out _entity))
			{
				return;
			}
			
			
			if (Input.GetButtonDown("Fire1"))
			{
				spawnBullet(_direction);
			}
			
		}
		
		public void FixedUpdate()
		{
			float hor = Input.GetAxis("Horizontal");
			float ver = Input.GetAxis("Vertical");
			
			_input.x = hor;
			_input.z = ver;
			
			_direction = new Vector3(hor, 0, ver);
			
			_rigidbody.velocity = _speed * Time.fixedDeltaTime * _direction.normalized;
			if (_rigidbody.velocity == Vector3.zero)
			{
				_velocity = 0;
				
			}
			else
			{
				_velocity = _rigidbody.velocity.sqrMagnitude;
				_entityManager.SetComponentData(_entity, new LocalTransform
				{
					Position = this.transform.position,
					Rotation = this.transform.rotation,
					Scale = this.transform.localScale.y,
				});
			}
		}

		private void spawnBullet(Vector3 vector3)
		{
			
		}
	}
