using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using System.Linq;

public class GameManager : MonoBehaviour
{
	#region Singleton
	public static GameManager Instance;
	
	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != null | Instance != this)
		{
			DestroyImmediate(gameObject);
		}
	}
	
	[Header("Controller")]
	public List<BaseController> Controllers = new List<BaseController>();
	
	/// <summary>
	/// Get specific controller by generic parameter
	/// </summary>
	/// <typeparam name="T">controller name</typeparam>
	/// <returns></returns>
	public T GetController<T>() where T : BaseController
	{
		return Controllers.OfType<T>().FirstOrDefault();
	}
	
	#endregion
	
	#region ECS grids
	private EntityManager _entityManager;
	[SerializeField]
	private bool _showGizmo;
	[SerializeField]
	private bool _showText;
	public Color FineColor = Color.white;
	public Color BadColor = Color.red;
	CollisionFilter _obstacleFilter = new CollisionFilter
	{
		BelongsTo = 1u << 6,
		CollidesWith = uint.MaxValue,
	};
	public void OnDrawGizmos()
	{
		if (!_showGizmo || !_isReady) { return; }
		
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(GridSingleton)}).TryGetSingletonEntity<GridSingleton>(out Entity gridEntity))
		{
			return;
		}
		
		var gridBuffer = _entityManager.GetBuffer<GridBuffer>(gridEntity);
		var gridSingleton = _entityManager.GetComponentData<GridSingleton>(gridEntity);
		
		for (int i = 0; i < gridBuffer.Length; i++)
		{
			Gizmos.color = gridBuffer[i].Value.IsWalkable ? FineColor : BadColor;
			
			Gizmos.DrawWireCube((Vector3)gridBuffer[i].Value.GetFloat3, new Vector3(gridSingleton.Spacing-0.1f, 0, gridSingleton.Spacing-0.1f));
			#if UNITY_EDITOR
			if (_showText)
			{
				int id = gridSingleton.GetIdFromPos(gridBuffer[i].Value.Pos);
				float2 pos = gridSingleton.GetPosFromId(id);
				UnityEditor.Handles.Label((Vector3)gridBuffer[i].Value.GetFloat3, $"{id}\n{pos.x},{pos.y}");
			}
			#endif
		}
	}
	
	#endregion
	
	public ENUM_GAME_STATE GameState;
	
	private bool _isReady;
	
	public void Start()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_isReady = true;
		
		foreach (BaseController item in Controllers)
		{
			item.IsInit = true;
			item.Init();	
		}
	}
	
	public void Update()
	{
		readHitData();
	}

	private void readHitData()
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(HitBufferDataMono) })
			.TryGetSingletonBuffer<HitBufferDataMono>(out var hitBuffer))
		{
			return;
		}
		
		if (hitBuffer.Length < 0) { return; }
		
		for (int i = hitBuffer.Length - 1; i >= 0 ; i--)
		{
			hitBuffer[i].Weapon.Value.OnHit(hitBuffer[i].Hit);
		}
		
		hitBuffer.Clear();
	}

	public void UpdateGridByMovement(float3 playerPos)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(GridSingleton)})
			.TryGetSingletonEntity<GridSingleton>(out Entity gridEntity))
		{
			return;
		}
		
		var gridSingleton = _entityManager.GetComponentData<GridSingleton>(gridEntity);
		float distancesq = math.distancesq(gridSingleton.Origin, playerPos.xz) ;
		if (math.any(distancesq >= gridSingleton.DistanceUpdateCheck))
		{
			gridSingleton.Origin = math.ceil(playerPos.xz);
			_entityManager.SetComponentData(gridEntity, gridSingleton);
			
			BakeGrid(_entityManager);
		}
	}
	
	public void BakeGrid(EntityManager entityManager)
	{
		if (!_entityManager.CreateEntityQuery(new ComponentType[] { typeof(GridSingleton)}).TryGetSingletonEntity<GridSingleton>(out Entity gridEntity))
		{
			return;
		}
		
		var gridBuffer = entityManager.GetBuffer<GridBuffer>(gridEntity);
		var gridSingleton = entityManager.GetComponentData<GridSingleton>(gridEntity);
		
		entityManager.CreateEntityQuery(new ComponentType[] { typeof(PhysicsWorldSingleton)}).TryGetSingleton(out PhysicsWorldSingleton world);
		gridBuffer.Clear();
		
		foreach (var item in BakeWalkable( world, gridSingleton))
		{
			gridBuffer.Add(new GridBuffer
			{
				Value = item,
			});
		}
		
	}
	
	public PathNode[] BakeWalkable(PhysicsWorldSingleton world, GridSingleton gridSingleton)
	{
		
		PathNode[] partitions = new PathNode[Mathf.CeilToInt(gridSingleton.Count)];
		// dont let 0 spacing
		if (gridSingleton.Spacing <= 0.1f)
		{
			return partitions;
		}
		
		// Draw a division
		int counter = 0;
		for (int i = 0; i < gridSingleton.Division.x; i++)
		{
			for (int j = 0; j < gridSingleton.Division.y; j++)
			{
				float midX = gridSingleton.GetMidX(i);
				float midY = gridSingleton.GetMidY(j);
				
				float2 pos = new float2(midX, midY) + gridSingleton.Origin;
				int index = gridSingleton.GetIdFromPos(pos);

				partitions[index] = new PathNode
				{
					Pos = pos,
					IsWalkable =  !world.CheckSphere(new float3(pos.x , 0, pos.y), gridSingleton.ObstacleCheckRadius, _obstacleFilter),
					Index = index,
					ComeFromIndex = -1,
					
					GCost = int.MaxValue,
				};
				counter++;
			}
		}
		// Debug.Log(gridSingleton.Count);
		
		return partitions;
	}

	
}
