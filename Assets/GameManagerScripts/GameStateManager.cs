using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] public GameObject player;
    [SerializeField] public GameObject agent;

    [Header("Manager Scripts")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObjectManager gameObjectManager;

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
        if (agent.GetComponent<MonsterStateManager>().currentState == MonsterStateManager.MonsterState.Fleeing)
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
        uiManager.HideCurrentScreen();
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerInputHandler>().enabled = true;
        agent.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 10f;
    }

    public void PauseGameObjects()
    {
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerInputHandler>().enabled = false;
        agent.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 0f;
    }
}
