using TMPro;
using UnityEngine;

public class UI : MonoBehaviour {
	public static UI Instance { get; private set; }
	public GameObject playingUI;
	private bool _hideUI;
	public Hotbar hotbar;
	public Hotbar hotbar2;
	public LoadingScreen loadingScreen;
	public Console console;
	public TextMeshProUGUI errorText;
	public CanvasGroup errorCanvasGroup;
	private float _errorTimer;

	public void Initialize() {
		Instance = this;
		hotbar.Initialize();
		hotbar2.Initialize();
		loadingScreen.Initialize();
		errorCanvasGroup.gameObject.SetActive(false);
	}

	public void UpdateUI() {
		hotbar.UpdateHotbar();
		hotbar2.UpdateHotbar();
		if (!Input.GetKeyDown(KeyCode.F1)) return;
		_hideUI = !_hideUI;
		playingUI.gameObject.SetActive(!_hideUI);

		if (Input.GetKeyDown(KeyCode.Slash)) {
			console.gameObject.SetActive(true);
		}

		if (console.gameObject.activeSelf) {
			console.UpdateConsole();
		}

		if (_errorTimer > 0) {
			errorCanvasGroup.gameObject.SetActive(true);
			_errorTimer -= Time.deltaTime;
			errorCanvasGroup.alpha = Mathf.Clamp01(_errorTimer);
		}
		else {
			errorCanvasGroup.gameObject.SetActive(false);
		}
	}

	public void ShowError(string text, float duration) {
		errorText.text = text;
		_errorTimer = duration;
	}
}