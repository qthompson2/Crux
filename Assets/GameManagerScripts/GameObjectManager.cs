using UnityEngine;

public class GameObjectManager : MonoBehaviour
{
    [Header("Start Positions")]
    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField] private Vector3 agentStartPosition;

    private GameStateManager gameStateManager;

    private void Awake()
    {
        gameStateManager = GetComponent<GameStateManager>();
    }

    public void ResetGameObjects()
    {
        GameObject player = gameStateManager.player;
        GameObject agent = gameStateManager.agent;
        if (player != null)
        {
            player.transform.position = playerStartPosition;
            player.transform.rotation = Quaternion.identity;
        }

        if (agent != null)
        {
            agent.transform.position = agentStartPosition;
            agent.transform.rotation = Quaternion.identity;
        }
    }
}
