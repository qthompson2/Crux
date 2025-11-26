using UnityEngine;
using System.Collections.Generic;

public class PlayerStateManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public PlayerInputHandler inputHandler;
    public StaminaManager staminaManager;
    public Transform cameraTransform;
    public List<Animator> playerAnimators;

    [Header("Current State")]
    public PlayerBaseState currentState;

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
        currentState = idleState;
        UpdateAnimatorState(currentState.GetType().Name, true);
    }

    void Update()
    {
        //Debug.Log("Current Stamina: " + staminaManager.currentStamina);
        if (currentState != null)
        {
            currentState?.UpdateState(this);
            UpdateAnimatorFloat("Y", inputHandler.MoveInput.y);
        }
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState == newState)
            return;

        currentState?.ExitState(this);
        UpdateAnimatorState(currentState.GetType().Name, false);
        currentState = newState;
        Debug.Log("Player State switched to: " + currentState.GetType().Name);
        currentState?.EnterState(this);
        UpdateAnimatorState(currentState.GetType().Name, true);
    }

    void UpdateAnimatorState(string state, bool enable)
    {
        foreach (Animator anim in playerAnimators)
        {
            anim?.SetBool(state, enable);
        }
    }

    void UpdateAnimatorFloat(string floatName, float value)
    {
        foreach (Animator anim in playerAnimators)
        {
            anim?.SetFloat(floatName, value);
        }
    }
}
