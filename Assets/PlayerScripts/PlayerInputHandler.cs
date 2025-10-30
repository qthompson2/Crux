using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMovement inputActions;

    // === MOVEMENT VALUES ===
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    // === ACTION FLAGS ===
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool ClimbHeld { get; private set; }

    void Awake()
    {
        inputActions = new PlayerMovement();

        // --- MOVEMENT ---
        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => MoveInput = Vector2.zero;

        // --- LOOK ---
        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => LookInput = Vector2.zero;

        // --- JUMP ---
        inputActions.Player.Jump.performed += _ => JumpPressed = true;
        inputActions.Player.Jump.canceled += _ => JumpPressed = false;

        // --- SPRINT ---
        inputActions.Player.Sprint.performed += ctx => SprintHeld = ctx.ReadValueAsButton();
        inputActions.Player.Sprint.canceled += _ => SprintHeld = false;

        // --- CROUCH ---
        inputActions.Player.Crouch.performed += _ => CrouchHeld = true;
        inputActions.Player.Crouch.canceled += _ => CrouchHeld = false;

        // --- CLIMB ---
        inputActions.Player.Climb.performed += _ => ClimbHeld = true;
        inputActions.Player.Climb.canceled += _ => ClimbHeld = false;
    }

    void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }
}
