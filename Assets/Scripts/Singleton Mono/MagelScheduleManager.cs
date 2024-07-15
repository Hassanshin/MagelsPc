using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class MagelSchedule : IComparable<MagelSchedule>
{
	public string Name;
	public Vector2Int Time;
	[Header("Data")]
	[TextArea(2, 5)]
	public string Description;
	public int EnergyConsumed;
	public Vector2Int HalfSize;
	public List<Vector2Int> EnemyChance;
	public List<Entity> SpawnedEntity;

    public int CompareTo(MagelSchedule other)
    {
        return Time.x < other.Time.x ? -1 : 1;
    }
}

public class MagelScheduleManager : BaseController
{
	private TimeManager _timeManager;
	private SpawnerManager _spawnerManager;
	[SerializeField]
	private int _winSeconds = 60;
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
			if (_schedules[i].Time.x == timer)
			{
				startSchedule(_schedules[i]);
			}
			
			if (_schedules[i].Time.y == timer)
			{
				stopSchedule(_schedules[i]);
			}
		}
		
		if (timer == _winSeconds)
		{
			GameManager.Instance.GameOver(true);
		}
	}
	
	[ContextMenu("sort")]
	public void SortSchedule()
	{
		_schedules.Sort();
	}
	
	private void stopSchedule(MagelSchedule magelSchedule)
	{
		// Debug.Log("remove" + magelSchedule.HalfSize);
	}

	private void startSchedule(MagelSchedule magelSchedule)
	{
		_spawnerManager.Build(magelSchedule);
		
		GameManager.Instance.GetController<UiManager>().SpawnToaster(magelSchedule);
	}
}
