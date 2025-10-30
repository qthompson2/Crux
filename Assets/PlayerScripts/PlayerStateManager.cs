using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public PlayerInputHandler inputHandler;
    public Transform cameraTransform;

    [Header("Current State")]
    public BaseState currentState;

    // References to all available states
    public IdleState idleState = new IdleState();
    public WalkingState walkingState = new WalkingState();
    public SprintingState sprintingState = new SprintingState();
    public JumpingState jumpingState = new JumpingState();
    public FallingState fallingState = new FallingState();
    public ClimbingState climbingState = new ClimbingState();
    public CrouchingState crouchingState = new CrouchingState();
    public MantlingState mantlingState = new MantlingState();
    public SlidingState slidingState = new SlidingState();
    public LandingState landingState = new LandingState();

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Start()
    {
        SwitchState(idleState);
    }

    void Update()
    {
        if (currentState != null)
            currentState?.UpdateState(this);
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdateState(this);
    }

    public void SwitchState(BaseState newState)
    {
        if (currentState == newState)
            return;

        currentState?.ExitState(this);
        currentState = newState;
        Debug.Log("Player State switched to: " + currentState.GetType().Name);
        currentState?.EnterState(this);
    }
}
