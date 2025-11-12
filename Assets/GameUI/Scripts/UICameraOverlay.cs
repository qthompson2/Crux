using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UICameraOverlay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public Image recordingDot;
    [SerializeField] public TMP_Text dateTimeText;

    [Header("Recording Settings")]
    [SerializeField] public float blinkInterval = 0.75f;

    private float elapsedTime = 0f;
    private float blinkTimer = 0f;
    private bool isDotVisible = true;
    private DateTime startTime = new DateTime(1, 1, 1, 12, 0, 0);

    // Update is called once per frame
    void Update()
    {
        // Update elapsed time
        elapsedTime += Time.deltaTime;
        blinkTimer += Time.deltaTime;

        DateTime simulatedTime = startTime.AddSeconds(elapsedTime);
        dateTimeText.text = simulatedTime.ToString("HH:mm:ss tt");

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
