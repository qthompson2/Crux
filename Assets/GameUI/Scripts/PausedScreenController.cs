using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PausedScreenController : MonoBehaviour
{
    [SerializeField] private GameObject CameraOverlayObject;
    [SerializeField] private TMP_Text PlayBackTime;
    [SerializeField] private Button HelpButton;
    [SerializeField] private GameObject HelpScreen;
	[SerializeField] private string ScreenName;
	[SerializeField] private AudioSource buttonPress;
    private UICameraOverlay CameraOverlay;
	private UIManager UI;

	void Start()
	{
		HelpButton.onClick.AddListener(OnHelpButtonPressed);
        CameraOverlay = CameraOverlayObject.GetComponent<UICameraOverlay>();
		UI = GameObject.Find("GameManager").GetComponent<UIManager>();
		buttonPress.ignoreListenerPause = true;
	}

	void Update()
	{
        DateTime startTime = new(1, 1, 1, 0, 0, 0);
        DateTime simulatedTime = startTime.AddSeconds(CameraOverlay.GetPlaybackTime());
		PlayBackTime.text = simulatedTime.ToString("HH:mm:ss");
	}

	public void OnHelpButtonPressed()
	{
		buttonPress.Play();
		if (ScreenName == "Pause")
		{
			HelpScreen.GetComponent<HelpScreenController>().SetReturnButtonOnPress(() => {buttonPress.Play(); UI.ShowPauseMenu();});
		}
		else if (ScreenName == "Lose")
		{
			HelpScreen.GetComponent<HelpScreenController>().SetReturnButtonOnPress(() => {buttonPress.Play(); UI.ShowLoseScreen();});
		}
		UI.ShowHelpScreen();
	}
}
