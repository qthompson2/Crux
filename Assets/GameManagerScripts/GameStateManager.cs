using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] public GameObject player;
    [SerializeField] private GameObject monsters;
    [SerializeField] private GameObject cameraOverlay;

    [Header("Manager Scripts")]
    [SerializeField] private UIManager uiManager;
    private float oldStaminaRegen;

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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        uiManager.HideCurrentScreen();
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        player.GetComponent<StaminaManager>().staminaRegenRate = oldStaminaRegen;
        monsters.GetComponent<MonsterManager>().Resume();
        cameraOverlay.GetComponent<UICameraOverlay>().TogglePause();
    }

    public void PauseGameObjects()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;
        (oldStaminaRegen, player.GetComponent<StaminaManager>().staminaRegenRate) = (player.GetComponent<StaminaManager>().staminaRegenRate, 0f);
        monsters.GetComponent<MonsterManager>().Pause();
        cameraOverlay.GetComponent<UICameraOverlay>().TogglePause();
    }

    public void ResetGameObjects()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
