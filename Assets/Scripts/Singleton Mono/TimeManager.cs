using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimeManager : BaseController
{
	GameManager _gameManager;
	[SerializeField]
	private int _timer;
	public int Timer => _timer;
	
	public UnityEvent<int> OnTimeUpdate = new();
	
	public override void Init()
	{
		_gameManager = GameManager.Instance;
		StartCoroutine(updateTimer());
	}
	
	public void Update()
	{
		if (_gameManager.GameState == ENUM_GAME_STATE.Playing)
		{
			updateTimer();
		}
	}

	private IEnumerator updateTimer()
	{
		while (_gameManager.GameState != ENUM_GAME_STATE.Playing)
		{
			yield return new WaitForSeconds(1);
			_timer++;
			
			OnTimeUpdate?.Invoke(_timer);
		}
	}
}
