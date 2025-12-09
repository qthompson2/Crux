using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] public GameObject player;
    [SerializeField] private GameObject monsters;
    [SerializeField] private GameObject yetis;
    [SerializeField] private GameObject cameraOverlay;

    [Header("Manager Scripts")]
    [SerializeField] private UIManager uiManager;
    private float oldStaminaRegen;
    private int hasShownControls;
    private bool shownWin = false;
    private bool shownLose = false;

	void Start()
	{
        AudioListener.pause = false;
		hasShownControls = PlayerPrefs.GetInt("hasShownControls", 0);

        if (hasShownControls == 0)
		{
            PauseGameObjects();
            uiManager.helpScreen.GetComponent<HelpScreenController>().SetReturnButtonText("Continue");
            uiManager.helpScreen.GetComponent<HelpScreenController>().SetReturnButtonOnPress(delegate () { uiManager.HideCurrentScreen(); ResumeGameObjects(); uiManager.helpScreen.GetComponent<HelpScreenController>().ResetReturnButtonText(); });
			uiManager.ShowHelpScreen();
            PlayerPrefs.SetInt("hasShownControls", 1);
		}
	}
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiManager.getCurrentScreen() == uiManager.pauseMenuScreen)
            {
                uiManager.HideCurrentScreen();
                ResumeGameObjects();
            }
            else
            {
                uiManager.ShowPauseMenu();
                PauseGameObjects();
            }
        }
        else if (player.GetComponent<StaminaManager>().maxCap == 0)
        {
            if (uiManager.getCurrentScreen() != uiManager.pauseMenuScreen && !shownLose)
			{
				uiManager.ShowLoseScreen();
                shownLose = true;
			}
            PauseGameObjects();
        }
        else if (player.transform.position.y > 400)
        {
            if (uiManager.getCurrentScreen() != uiManager.winScreen && !shownWin)
			{
				uiManager.ShowWinScreen();
                shownWin = true;
			}
            PauseGameObjects();
        }
    }

    public void ResumeGameObjects()
    {
        AudioListener.pause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        uiManager.HideCurrentScreen();
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<StaminaManager>().staminaRegenRate = oldStaminaRegen;
        monsters.GetComponent<MonsterManager>().Resume();
        yetis.GetComponent<YetiManager>().Resume();
        cameraOverlay.GetComponent<UICameraOverlay>().Resume();
    }

    public void PauseGameObjects()
    {
        AudioListener.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;
        (oldStaminaRegen, player.GetComponent<StaminaManager>().staminaRegenRate) = (player.GetComponent<StaminaManager>().staminaRegenRate, 0f);
        monsters.GetComponent<MonsterManager>().Pause();
        yetis.GetComponent<YetiManager>().Pause();

        cameraOverlay.GetComponent<UICameraOverlay>().Pause();
    }

    public void ResetGameObjects()
    {
        AudioListener.pause = false;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
