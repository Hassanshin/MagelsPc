using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : BaseController
{
	[SerializeField]
	private Slider _healthSlider;
	[SerializeField]
	private Slider _energySlider;
	
	[SerializeField]
	private RectTransform _losePanel;
	[SerializeField]
	private RectTransform _winPanel;
	
	[Header("toaster")]
	[SerializeField]
	private Image _toasterPrefab;
	[SerializeField]
	private RectTransform _toasterParent;
	
	[Header("weapon")]
	[SerializeField]
	private Image[] _ammoArray;
	[SerializeField]
	private TextMeshProUGUI _ammoText;
	
	public override void Init()
	{
		
	}
	
	public void ShowAmmo(int amount, bool isReloading)
	{
		if (isReloading)
		{
			_ammoText.text = "Reloading";
		}
		else 
		{
			_ammoText.text = "";
		}
		
		for (int i = 0; i < _ammoArray.Length; i++)
		{
			_ammoArray[i].gameObject.SetActive(i < amount);
		}
	}
	
	public void GameOver(ENUM_GAME_STATE state)
	{
		switch (state)
		{
			case ENUM_GAME_STATE.Winning:
				_winPanel.gameObject.SetActive(true);
				break;
			case ENUM_GAME_STATE.Losing:
				_losePanel.gameObject.SetActive(true);
				break;
		}
	}
	
	public void SpawnToaster(MagelSchedule schedule)
	{
		var spawnedUi = Instantiate(_toasterPrefab, _toasterParent);
		spawnedUi.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = schedule.Name + " started";
		spawnedUi.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = schedule.Description;
		
		Destroy(spawnedUi.gameObject, 3f);
	}
	
	public void SpawnToaster(string text, string textDesc, Color color)
	{
		color.a = 0.5f;
		var spawnedUi = Instantiate(_toasterPrefab, _toasterParent);
		spawnedUi.color = color;
		spawnedUi.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
		spawnedUi.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = textDesc;
		
		Destroy(spawnedUi.gameObject, 3f);
	}
	
	public void UpdateSliderHealth(float percent)
	{
		_healthSlider.value = percent;
	}
	
	public void UpdateSliderEnergy(float percent)
	{
		_energySlider.value = percent;	
	}
	
	public void RestartTheGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
