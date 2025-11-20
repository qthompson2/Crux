using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private static readonly WaitForSeconds _waitForSeconds0_01 = new(0.01f);
	[Header("UI Screens")]
    [SerializeField] public GameObject pauseMenuScreen;
    [SerializeField] public GameObject winScreen;
    [SerializeField] public GameObject loseScreen;
    [SerializeField] public GameObject helpScreen;
    [SerializeField] private GameObject fadeOutPanel;
    private Image panelImage;
    private GameObject currentScreen;

	void Start()
	{
		panelImage = fadeOutPanel.GetComponent<Image>();
	}

	private void ShowScreen(GameObject screen)
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }
        currentScreen = screen;
        if (currentScreen != null)
        {
            currentScreen.SetActive(true);
        }
    }

    public void ShowPauseMenu()
    {
        ShowScreen(pauseMenuScreen);
    }
    public void ShowWinScreen()
    {
        StartCoroutine(FadeOutToScreen(winScreen));
    }
    public void ShowLoseScreen()
    {
        ShowScreen(loseScreen);
    }

    public void ShowHelpScreen()
	{
		ShowScreen(helpScreen);
	}

    public void HideCurrentScreen()
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
            currentScreen = null;
        }
    }

    public GameObject getCurrentScreen()
    {
        return currentScreen;
    }

    private IEnumerator FadeOutToScreen(GameObject screen)
	{
        while (panelImage.color.a < 1)
        {
			panelImage.color = new(panelImage.color.r, panelImage.color.g, panelImage.color.b, panelImage.color.a + 0.01f);
            yield return _waitForSeconds0_01;
        }

        ShowScreen(screen);
        panelImage.color = new(panelImage.color.r, panelImage.color.g, panelImage.color.b, 0);
	}
}
