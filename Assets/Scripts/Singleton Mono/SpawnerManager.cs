using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using System;
using Unity.Collections;
using Tertle.DestroyCleanup;
using BoxCollider = UnityEngine.BoxCollider;
using System.Collections;

public class SpawnerManager : BaseController
{
	EntityManager _entityManager;
	DynamicBuffer<EnemyDataBufferSingleton> _spawnEnemyDatas;
	DynamicBuffer<DataBlockDataBufferSingleton> _spawnDataBlockDatas;
	DynamicBuffer<PowerUpsDataBufferSingleton> _powerUpsDatas;
	DynamicBuffer<WallDataBufferSingleton> _spawnWallDatas;
	
	[Header("Data")]
	[SerializeField]
	private Vector3 _defaultPosCornerStorage;
	[SerializeField]
	private Vector3 _defaultPosCornerProcess;
	[SerializeField]
	private Transform _cornerProcess;
	[SerializeField]
	private Transform _cornerStorage;
	CollisionFilter _obstacleFilter = new CollisionFilter
	{
		BelongsTo = 1u << 6,
		CollidesWith = 1u << 6,
	};
	
	[Header("Mono colliders")]
	[SerializeField]
	private BoxCollider _monoWall;
	[SerializeField]
	private BoxCollider _monoDoors;
	
	public override void Init()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_cornerProcess.transform.position = _defaultPosCornerProcess;	
		_cornerStorage.transform.position = _defaultPosCornerStorage;	
	}
	
	public void Build(MagelSchedule schedule)
	{
		Vector2Int size = schedule.HalfSize;
		Vector3Int center = new Vector3Int(
			Mathf.CeilToInt(_cornerProcess.position.x) + size.x, 0, 
			Mathf.CeilToInt(_cornerProcess.position.z));
		
		GroupSpawn(schedule, center);
		
		_cornerProcess.position = new Vector3( 1 + _cornerProcess.position.x + size.x * 2, 0, _cornerProcess.position.z);
		
		int length = size.x;
		
		// end wall
		for (int i = -length; i < length + 1; i++)
		{
			SpawnWall(0, new Vector3Int(center.x + i, 0, center.z - size.y));
		}
		
		for (int i = -length; i < length + 1; i++)
		{
			if (i < 2 && i > -2)
			{
				if (i == 0)
				{
				    SpawnMonoDoor(schedule, new Vector3Int(center.x + i, 0, center.z + 1));
				}
				continue;
			}

			SpawnWall(1, new Vector3Int(center.x + i, 0, center.z + 1));
		}
		
		
		// right left wall
		length = size.y;
		for (int i = 0; i < length; i++)
		{
			SpawnWall(0, new Vector3Int(center.x + size.x, 0, center.z - i));
		}
		for (int i = 0; i < length; i++)
		{
			SpawnWall(0, new Vector3Int(center.x - size.x, 0, center.z - i));
		}
		
		StartCoroutine(DelayedActivation(schedule, 0.4f));
	}

	

	public IEnumerator DelayedActivation(MagelSchedule schedule, float delay)
	{
		yield return new WaitForSeconds(delay);
		GameManager.Instance.BakeGrid(_entityManager);
		yield return new WaitForSeconds(delay);
		
		
	}
	
	public void GroupSpawn(MagelSchedule schedule, Vector3Int center)
	{
		schedule.SpawnedEntity = new System.Collections.Generic.List<Entity>();
		for (int i = 0; i < schedule.EnemyChance.Count; i++)
		{
			int randomAmount = UnityEngine.Random.Range(schedule.EnemyChance[i].x, schedule.EnemyChance[i].y + 1);
			
			for (int j = 0; j < randomAmount; j++)
			{
				float3 randomOffset = getRandomOffset(schedule);
				
				schedule.SpawnedEntity.Add(SpawnEnemy(i, (float3)(Vector3)center + randomOffset));
			}

			// Debug.Log($"{schedule.SpawnedEntity.Count} {-schedule.HalfSize.x} <-> {schedule.HalfSize.x +1} \t {center.z} <-> {schedule.HalfSize.y}");
		}
	}

	private static float3 getRandomOffset(MagelSchedule schedule)
	{
		return new float3(
			UnityEngine.Random.Range(-schedule.HalfSize.x + 1, schedule.HalfSize.x - 2), 0,
			UnityEngine.Random.Range(-schedule.HalfSize.y + 1, 0));
	}
	
	public void RandomSpawnPowerUps(float2 pos, int chance)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(PowerUpsDataBufferSingleton) })
			.TryGetSingletonBuffer(out _powerUpsDatas))
		{
			return;
		}
		
		bool willDrop = UnityEngine.Random.Range(1, 101) <= chance;
		
		if (!willDrop)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, _powerUpsDatas.Length);
		
		SpawnPowerUps(index, new float3(pos.x, 0, pos.y));
	}

	public void RemoveWall(Vector3Int vector3Int)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(PhysicsWorldSingleton)})
			.TryGetSingleton(out PhysicsWorldSingleton world))
		{
			return;
		}
		float3 pos = vector3Int - new Vector3(0.5f, 0, 0.5f);
		// Debug.Log(pos);
		NativeList<DistanceHit> hit = new NativeList<DistanceHit>(Allocator.Temp);
		if (world.OverlapSphere(pos, 0.25f, ref hit, _obstacleFilter))
		{
			// Debug.Log(hit);
			
			for (int i = 0; i < hit.Length; i++)
			{
				var hitEntity = hit[i].Entity;
				_entityManager.SetComponentEnabled<DestroyTag>(hitEntity, true);
				// Debug.Log(hitEntity);
			}
		}
		hit.Dispose();
	}

	#region basic spawning
	
	public GameObject SpawnMonoDoor(MagelSchedule schedule, Vector3Int pos)
	{
		GameObject door = Instantiate(_monoDoors, pos - spawnOffsetToGrid, quaternion.identity).gameObject;
		
		door.TryGetComponent<OnTriggerEnterEvent>(out var comp);
		comp.TriggerEnter.AddListener((col) =>
		{
			for (int i = 0; i < schedule.SpawnedEntity.Count; i++)
			{
				_entityManager.SetEnabled(schedule.SpawnedEntity[i], true);
			}
		});
		
		return door ;
	}
	
	public Entity SpawnWall(int index, Vector3Int pos)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(WallDataBufferSingleton) })
			.TryGetSingletonBuffer(out _spawnWallDatas))
		{
			return Entity.Null;
		}
		
		// mono wall invisible
		Instantiate(_monoWall, pos - spawnOffsetToGrid, quaternion.identity);
		
		// ecs wall
		Entity spawned = _entityManager.Instantiate(_spawnWallDatas[index].Entity);
		_entityManager.SetComponentData(spawned, new LocalTransform
		{
			Position = pos - spawnOffsetToGrid,
			Rotation = quaternion.identity,
			Scale = 1,
		});
		return spawned;
	}

	private static Vector3 spawnOffsetToGrid
	{
		get
		{
			return new Vector3(0.5f, 0, 0.5f);
		}
	}

	public Entity SpawnEnemy(int index, float3 pos) 
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(EnemyDataBufferSingleton) })
			.TryGetSingletonBuffer(out _spawnEnemyDatas))
		{
			return Entity.Null;
		}
		
		Entity spawned = _entityManager.Instantiate(_spawnEnemyDatas[index].Entity);
		_entityManager.SetComponentData(spawned, new LocalTransform
		{
			Position = pos,
			Rotation = quaternion.identity,
			Scale = 0.5f,
		});
		_entityManager.SetEnabled(spawned, false);
		
		return spawned;
	}
	public Entity SpawnDataBlock(int index, float3 pos) 
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(DataBlockDataBufferSingleton) })
			.TryGetSingletonBuffer(out _spawnDataBlockDatas))
		{
			return Entity.Null;
		}
		
		Entity spawned = _entityManager.Instantiate(_spawnDataBlockDatas[index].Entity);
		_entityManager.SetComponentData(spawned, new LocalTransform
		{
			Position = pos,
			Rotation = quaternion.identity,
			Scale = 1,
		});
		return spawned;
	}

	public Entity SpawnPowerUps(int index, float3 pos)
	{
		Entity spawned = _entityManager.Instantiate(_powerUpsDatas[index].Entity);
		_entityManager.SetComponentData(spawned, new LocalTransform
		{
			Position = pos,
			Rotation = quaternion.identity,
			Scale = 1,
		});
		return spawned;
	}
	#endregion
}
