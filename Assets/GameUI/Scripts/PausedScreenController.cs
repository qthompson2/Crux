using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PausedScreenController : MonoBehaviour
{
    [SerializeField] private GameObject CameraOverlayObject;
    [SerializeField] private TMP_Text PlayBackTime;
    [SerializeField] private Button HelpButton;
    [SerializeField] private GameObject HelpScreen;
    private UICameraOverlay CameraOverlay;

	void Start()
	{
		HelpButton.onClick.AddListener(OnHelpButtonPressed);
        CameraOverlay = CameraOverlayObject.GetComponent<UICameraOverlay>();
	}

	void Update()
	{
        DateTime startTime = new(1, 1, 1, 0, 0, 0);
        DateTime simulatedTime = startTime.AddSeconds(CameraOverlay.GetPlaybackTime());
		PlayBackTime.text = simulatedTime.ToString("HH:mm:ss");
	}

	public void OnHelpButtonPressed()
	{
		HelpScreen.SetActive(true);
	}
}
