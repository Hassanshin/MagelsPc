using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class MagelSchedule
{
	public string Name;
	public float StartTime;
	public float EndTime;
	public Vector2Int HalfSize;
	public List<Vector2Int> EnemyChance;
	public List<Entity> SpawnedEntity;
}

public class MagelScheduleManager : BaseController
{
	private TimeManager _timeManager;
	private SpawnerManager _spawnerManager;
	[SerializeField]
	private List<MagelSchedule> _schedules = new();
	private EntityManager _entityManager;

	public override void Init()
	{
		_timeManager = GameManager.Instance.GetController<TimeManager>();
		_spawnerManager = GameManager.Instance.GetController<SpawnerManager>();
		
		_timeManager.OnTimeUpdate.AddListener(UpdateSchedule);
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}
	
	public void UpdateSchedule(int timer)
	{
		for (int i = _schedules.Count - 1; i >= 0 ; i--)
		{
			if (_schedules[i].StartTime == timer)
			{
				startSchedule(_schedules[i]);
			}
			
			if (_schedules[i].EndTime == timer)
			{
				stopSchedule(_schedules[i]);
			}
		}
	}

	private void stopSchedule(MagelSchedule magelSchedule)
	{
		// Debug.Log("remove" + magelSchedule.HalfSize);
	}

	private void startSchedule(MagelSchedule magelSchedule)
	{
		Debug.Log("build  " + magelSchedule.HalfSize);
		_spawnerManager.Build(magelSchedule);
		
		
	}
}
