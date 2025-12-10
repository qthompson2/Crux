using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private static readonly WaitForSeconds _waitForSeconds0_01 = new(0.01f);
	[Header("UI Screens")]
    [SerializeField] public GameObject pauseMenuScreen;
    [SerializeField] public GameObject loseScreen;
    [SerializeField] public GameObject helpScreen;
    [SerializeField] private GameObject fadeOutPanel;
    [SerializeField] private AudioSource menuStatic;
    [SerializeField] private AudioSource buttonPress;
    private Image panelImage;
    private GameObject currentScreen;

	void Start()
	{
		panelImage = fadeOutPanel.GetComponent<Image>();
        menuStatic.ignoreListenerPause = true;
        buttonPress.ignoreListenerPause = true;
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
        if (!menuStatic.isPlaying)
		{
			menuStatic.Play();
		}
        ShowScreen(pauseMenuScreen);
    }
    public void ShowWinScreen()
    {
        StartCoroutine(FadeOut());
    }
    public void ShowLoseScreen()
    {
        if (!menuStatic.isPlaying)
		{
			menuStatic.Play();
		}
        ShowScreen(loseScreen);
    }

    public void ShowHelpScreen()
	{
        if (!menuStatic.isPlaying)
		{
			menuStatic.Play();
		}
		ShowScreen(helpScreen);
	}

    public void HideCurrentScreen()
    {
        menuStatic.Stop();
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

    private IEnumerator FadeOut()
	{
        panelImage.color = new(panelImage.color.r, panelImage.color.g, panelImage.color.b, 0);
        while (panelImage.color.a < 1)
        {
			panelImage.color = new(panelImage.color.r, panelImage.color.g, panelImage.color.b, panelImage.color.a + 0.01f);
            yield return _waitForSeconds0_01;
        }

        SceneManager.LoadScene("EndScene");
	}
}
