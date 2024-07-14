using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using System;
using Unity.Collections;
using Tertle.DestroyCleanup;

public class SpawnerManager : BaseController
{
	EntityManager _entityManager;
	DynamicBuffer<EnemyDataBufferSingleton> _spawnEnemyDatas;
	DynamicBuffer<DataBlockDataBufferSingleton> _spawnDataBlockDatas;
	DynamicBuffer<WallDataBufferSingleton> _spawnWallDatas;
	
	[SerializeField]
	private Transform _cornerProcess;
	[SerializeField]
	private Transform _cornerStorage;
	CollisionFilter _obstacleFilter = new CollisionFilter
	{
		BelongsTo = 1u << 6,
		CollidesWith = uint.MaxValue,
	};
	public override void Init()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;	
	}
	
	public void Build(Vector2Int size)
	{
		
		Vector3Int center = new Vector3Int(
			Mathf.CeilToInt(_cornerProcess.position.x) + size.x, 0, 
			Mathf.CeilToInt(_cornerProcess.position.z));
		
		_cornerProcess.position = new Vector3( 1 + _cornerProcess.position.x + size.x * 2, 0, _cornerProcess.position.z);
		
		int length = size.x;
		
		// end wall
		for (int i = -length; i < length + 1; i++)
		{
			SpawnWall(0, new Vector3Int(center.x + i, 0, center.z - size.y));
		}
		
		// remove door
		RemoveWall(new Vector3Int(center.x + 1, 0, center.z +1));
		RemoveWall(new Vector3Int(center.x    , 0, center.z +1));
		RemoveWall(new Vector3Int(center.x - 1, 0, center.z +1));
		
		
		
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
		
		GameManager.Instance.BakeGrid(_entityManager);
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
	public Entity SpawnWall(int index, Vector3Int pos)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(WallDataBufferSingleton) })
			.TryGetSingletonBuffer(out _spawnWallDatas))
		{
			return Entity.Null;
		}
		
		Entity spawned = _entityManager.Instantiate(_spawnWallDatas[index].Entity);
		_entityManager.SetComponentData(spawned, new LocalTransform
		{
			Position = pos - new Vector3(0.5f, 0, 0.5f),
			Rotation = quaternion.identity,
			Scale = 1,
		});
		return spawned;
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
			Scale = 1,
		});
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
	#endregion
}
