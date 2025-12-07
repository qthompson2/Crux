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
        if (currentState != null)
        {
            currentState?.UpdateState(this);
            float animationMultiplier = Mathf.Clamp(Mathf.Abs(inputHandler.MoveInput.y) + Mathf.Abs(inputHandler.MoveInput.x), 0.0f, 1.0f); // use abs so negative x and positive y don't cancel each other out
            UpdateAnimatorFloat("AnimationMultiplier", animationMultiplier * Mathf.Sign(inputHandler.MoveInput.y)); // get original sign of y to play animation depending on if y is + 0r -
        }
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState == newState)
            return;

        currentState?.ExitState(this);
        UpdateAnimatorState(currentState.GetType().Name, false);
        currentState = newState;
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
