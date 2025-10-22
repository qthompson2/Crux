using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Screens")]
    [SerializeField] public GameObject pauseMenuScreen;
    [SerializeField] public GameObject winScreen;
    [SerializeField] public GameObject loseScreen;
    private GameObject currentScreen;

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
        ShowScreen(winScreen);
    }
    public void ShowLoseScreen()
    {
        ShowScreen(loseScreen);
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
}
