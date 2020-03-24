using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SettingsManager : MonoBehaviour
{
	public Toggle fullscreenToggle;
	public Dropdown resolutionDropdown;
	public Dropdown qualityDropdown;

	private Resolution[] resolutions;
	private GameSettings gameSettings;

	private bool unsavedSettings = false;

	void OnEnable () {
		Debug.Log("init");
		gameSettings = new GameSettings();

		fullscreenToggle.onValueChanged.AddListener(delegate { onFullscreenToggle(); });
		resolutionDropdown.onValueChanged.AddListener(delegate { onResolutionChange(); });
		qualityDropdown.onValueChanged.AddListener(delegate { onQualityChange(); });

		resolutions = Screen.resolutions;

		List<string> optionData = new List<string>();
		foreach(Resolution res in resolutions) {
			optionData.Add(res.ToString());
		}
		resolutionDropdown.AddOptions(optionData);

		loadSettings();
		StartCoroutine(checkSaveSettings());
	}

	IEnumerator checkSaveSettings() {
		while(true) {
			if (unsavedSettings) {
				saveSettings();
				unsavedSettings = false;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void onFullscreenToggle() {
		Debug.Log("fullscreen change");
		gameSettings.fullscreen = fullscreenToggle.isOn;
		Screen.fullScreen = fullscreenToggle.isOn;
		unsavedSettings = true;
	}

	public void onResolutionChange() {
		Debug.Log("resolution change");
		gameSettings.resolution = resolutionDropdown.value;
		Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, gameSettings.fullscreen);
		unsavedSettings = true;
	}

	public void onQualityChange() {
		Debug.Log("quality change");
		Debug.Log(qualityDropdown.value);
		gameSettings.quality = qualityDropdown.value;
		QualitySettings.SetQualityLevel(qualityDropdown.value, true);
		unsavedSettings = true;
	}

	public void saveSettings() {
		Debug.Log("Save Settings");
		string jsonData = JsonUtility.ToJson(gameSettings, true);
		File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
	}

	public void loadSettings() {
		try {
			gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));
			qualityDropdown.value = gameSettings.quality;
			resolutionDropdown.value = gameSettings.resolution;
			fullscreenToggle.isOn = gameSettings.fullscreen;
		} catch (System.ArgumentException) {
			Debug.Log("failed");
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			resolutionDropdown.value = System.Array.IndexOf(resolutions, Screen.currentResolution);
			fullscreenToggle.isOn = Screen.fullScreen;
		}
	}
}
