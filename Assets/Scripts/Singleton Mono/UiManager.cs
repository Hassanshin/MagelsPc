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
	private Image _toasterImage;
	[SerializeField]
	private TextMeshProUGUI _toasterText;
	
	public override void Init()
	{
		
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
	
	public void UpdateToaster(MagelSchedule schedule)
	{
		_toasterImage.gameObject.SetActive(true);
		_toasterText.text = schedule.Name;
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
