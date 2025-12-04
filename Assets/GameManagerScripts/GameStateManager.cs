using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] public GameObject player;
    [SerializeField] private GameObject monsters;
    [SerializeField] private GameObject cameraOverlay;

    [Header("Manager Scripts")]
    [SerializeField] private UIManager uiManager;
    private float oldStaminaRegen;
    private int hasShownControls;

    [Header("Pause State")]
    public static bool IsPaused { get; private set; } // static property to track pause state, all scripts can access
    public static event Action<bool> OnPauseChanged; // Event to notify subscribers of pause state changes

    void Start()
	{
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
        if (player.GetComponent<StaminaManager>().maxCap == 0)
        {
            uiManager.ShowLoseScreen();
            PauseGameObjects();
        }
        if (player.transform.position.y > 400)
        {
            uiManager.ShowWinScreen();
            PauseGameObjects();
        }
    }

    public void ResumeGameObjects()
    {
        IsPaused = false; // Flag the game as resumed
        Debug.Log($"Game paused: {IsPaused}");
        OnPauseChanged?.Invoke(IsPaused); // Notify subscribers that the game is resumed

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        uiManager.HideCurrentScreen();
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<StaminaManager>().staminaRegenRate = oldStaminaRegen;
        monsters.GetComponent<MonsterManager>().Resume();
        cameraOverlay.GetComponent<UICameraOverlay>().Resume();
    }

    public void PauseGameObjects()
    {
        IsPaused = true; // Flag the game as paused
        Debug.Log($"Game paused: {IsPaused}");
        OnPauseChanged?.Invoke(IsPaused); // Notify subscribers that the game is paused

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;
        (oldStaminaRegen, player.GetComponent<StaminaManager>().staminaRegenRate) = (player.GetComponent<StaminaManager>().staminaRegenRate, 0f);
        monsters.GetComponent<MonsterManager>().Pause();
        cameraOverlay.GetComponent<UICameraOverlay>().Pause();
    }

    public void ResetGameObjects()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
