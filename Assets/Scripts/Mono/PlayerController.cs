using System;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
	private EntityManager _entityManager;
	private UiManager _uiManager;
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
	
	Vector3 _cursorWorldpoint = new Vector3();
	[SerializeField]
	private Transform _aimingTransform;
	[SerializeField]
	private ParticleSystem _vfxMuzzle;
	public void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_uiManager = GameManager.Instance.GetController<UiManager>();
		_cam = Camera.main;
		
	}
		
	public void Update()
	{
		_isReady = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(PlayerTag) }).TryGetSingletonEntity<PlayerTag>(out _entity);
		if (!_isReady) { return; }

		if (Input.GetButtonDown("Fire1"))
		{
			shoot();
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			reload();
		}

		updateUi();
		readPowerUpsData();
	}

	private void readPowerUpsData()
	{
		if (_entityManager.CreateEntityQuery(new ComponentType[] { typeof(PlayerGettingHitBufferToMono) })
			.TryGetSingletonBuffer<PlayerGettingHitBufferToMono>(out var gettingHitBuffer))
		{
			if (gettingHitBuffer.Length < 0) { return; }

			for (int i = gettingHitBuffer.Length - 1; i >= 0; i--)
			{
				applyPowerUp(gettingHitBuffer[i].PowerUps);

			}

			gettingHitBuffer.Clear();
		}
	}

	private void applyPowerUp(PowerUpsComponent powerUp)
	{
		_uiManager.SpawnToaster(powerUp.Type.ToString(), "updated modules", new Color(0.8f, 0.5f, 0.7f));
		switch (powerUp.Type)
		{
			case ENUM_POWER_UPS_TYPE.Health:
				// do in ecs
				break;
			case ENUM_POWER_UPS_TYPE.Damage:
				Weapons[0].BulletComp.Damage += powerUp.Amount;
				break;
			case ENUM_POWER_UPS_TYPE.Pierce:
				Weapons[0].BulletComp.Pierce += powerUp.Amount;
				break;
			case ENUM_POWER_UPS_TYPE.Spread:
				Weapons[0].MaxAngle += powerUp.Amount;
				break;
			case ENUM_POWER_UPS_TYPE.Bullet:
				Weapons[0].TotalShot += powerUp.Amount;
				break;
			case ENUM_POWER_UPS_TYPE.Duration:
				break;
		}
	}

	private void updateUi()
	{
		var stats = _entityManager.GetComponentData<StatsComponent>(_entity);
		_uiManager.UpdateSliderHealth(stats.Health.PercentageValue);
		_uiManager.UpdateSliderEnergy(stats.Energy.PercentageValue);
		
		if (stats.Health.Value <= 0)
		{
			GameManager.Instance.GameOver(false);
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
			
			// set current velocity as direction
			// transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
			
			_entityManager.SetComponentData(_entity, new LocalTransform
			{
				Position = this.transform.position,
				Rotation = this.transform.rotation,
				Scale = this.transform.localScale.y,
			});
			
			GameManager.Instance.UpdateGridByMovement(this.transform.position);
		}
		
		// facing
		Vector3 _mousePos = Input.mousePosition;
		_cursorWorldpoint = Camera.main.ScreenToWorldPoint(new Vector3(_mousePos.x, _mousePos.y, _cam.transform.position.y));
		_cursorWorldpoint.y = 0;
		transform.rotation = Quaternion.LookRotation((_cursorWorldpoint - this.transform.position).normalized);
	}

	private void shoot()
	{
		for (int i = 0; i < Weapons.Count; i++)
		{
			if (Weapons[i].Shoot())
            {
		        _vfxMuzzle.Play();
            }
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
