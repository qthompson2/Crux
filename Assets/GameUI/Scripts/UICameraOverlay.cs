using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UICameraOverlay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public Image recordingDot;
    [SerializeField] public TMP_Text timerText;

    [Header("Recording Settings")]
    [SerializeField] public float blinkInterval = 0.75f;

    private float elapsedTime = 0f;
    private float blinkTimer = 0f;
    private bool isDotVisible = true;

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        blinkTimer += Time.deltaTime;

        // Update timer text
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
        timerText.text = SpacedOut(string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));

        // Handle blinking dot
        if (blinkTimer >= blinkInterval)
        {
            isDotVisible = !isDotVisible;
            recordingDot.enabled = isDotVisible;
            blinkTimer = 0f;
        }
    }

    void OnEnable()
    {
        // Reset timers when enabled
        elapsedTime = 0f;
        blinkTimer = 0f;
        isDotVisible = true;
        if (recordingDot != null)
        {
            recordingDot.enabled = true;
        }
    }

    private string SpacedOut(string input)
    {
        return string.Join(" ", input.ToCharArray());
    }
}
