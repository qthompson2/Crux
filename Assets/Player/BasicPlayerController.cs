using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BasicPlayerController : MonoBehaviour, PlayerMovement.IMovementActions
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 7f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 5f;
    private float originalCameraY;
    private bool isCrouching = false;
    private bool crouchPressed = false;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float maxLookAngle = 80f;

    [Header("Climbing & Mantling Settings")]
    public float climbSpeed = 2f; // Base climbing speed
    public float climbDetectionDistance = 1f;
    public LayerMask climbableLayer;
    [Range(-90, 0)] public float minClimbAngle = -30f;
    [Range(0, 90)] public float maxClimbAngle = 80f;

    // Mantling parameters (for the pull-up action)
    public float mantleMaxHeight = 1.5f;
    public float mantleDuration = 0.3f;
    public LayerMask mantleLayer;

    [Header("Mantle Fine-Tuning")]
    [SerializeField, Range(0f, 0.2f)]
    float mantleVerticalOffset = 0.01f; // Adjust this in the Inspector!

    private CharacterController controller;
    private PlayerMovement inputActions;

    // Movement state
    private Vector2 movementInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpPressed;
    private bool sprintPressed = false;

    // Climbing/Mantling state
    private bool isClimbing = false;
    private bool climbPressed = false;
    private bool isMantling = false;
    private bool isClimbToggleActive = false;
    private Vector3 climbNormal;
    private Vector3 clingNormal;

    // NEW: Surface properties storage
    private ClimbableSurface currentSurface;
    private float currentClimbSpeedMultiplier = 1f;

    // Mouse look state
    private float xRotation = 0f;

    void Awake()
    {
        inputActions = new PlayerMovement();
        inputActions.Movement.SetCallbacks(this);
        controller = GetComponent<CharacterController>();

        originalCameraY = playerCamera.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mantleLayer.value == 0) mantleLayer = climbableLayer;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleClimbing();
        HandleMovement();
    }

    // --- INPUT HANDLING ---

    public void OnMovement(InputAction.CallbackContext context) => movementInput = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) => jumpPressed = context.performed;

    public void OnClimb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            climbPressed = true;
        }
        else if (context.canceled)
        {
            climbPressed = false;
        }
    }
    // ... (OnSprint and OnCrouch remain unchanged) ...
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintPressed = true;
        }
        else if (context.canceled)
        {
            sprintPressed = false;
        }
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            crouchPressed = true;
        }
    }

    // --- MOVEMENT & VIEW ---

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleCrouch()
    {
        if (crouchPressed)
        {
            if (isCrouching && CanStandUp()) isCrouching = false;
            else if (!isCrouching) isCrouching = true;
            crouchPressed = false;
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        float targetCameraY = isCrouching ? crouchHeight * 0.5f : originalCameraY;
        Vector3 cameraPos = playerCamera.localPosition;
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetCameraY, crouchTransitionSpeed * Time.deltaTime);
        playerCamera.localPosition = cameraPos;

        Vector3 center = controller.center;
        center.y = controller.height * 0.5f;
        controller.center = center;
    }

    bool CanStandUp()
    {
        Vector3 rayStart = transform.position + Vector3.up * (crouchHeight + 0.1f);
        float rayLength = standHeight - crouchHeight - 0.1f;
        return !Physics.Raycast(rayStart, Vector3.up, rayLength);
    }

    void HandleMovement()
    {
        if (isClimbing || isMantling) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        Vector3 move = (transform.right * movementInput.x + transform.forward * movementInput.y).normalized;

        float currentSpeed = GetCurrentSpeed();
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (jumpPressed && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpPressed = false;
            isCrouching = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    float GetCurrentSpeed()
    {
        if (isCrouching) return crouchSpeed;
        if (sprintPressed && movementInput.y > 0) return runSpeed;
        return walkSpeed;
    }

    // --- CLIMBING AND MANTLING LOGIC ---

    void HandleClimbing()
    {
        if (isMantling) return;

        bool surfaceFound = CheckForClimbableSurface();

        if (isClimbing)
        {
            if (surfaceFound && isClimbToggleActive)
            {
                PerformClimbing();
            }
            else
            {
                if (movementInput.y > 0.1f && movementInput.x == 0f && !isMantling && TryMantleFromClimbExit())
                {
                    return;
                }

                // If fall is initiated here, isClimbToggleActive must be set to false 
                // to allow the next button press to re-latch.
                isClimbToggleActive = false;
                StopClimbing(true);
            }
        }
    }

    // Returns TRUE if a climbable wall is hit
    bool CheckForClimbableSurface()
    {
        // Fail conditions
        if (isMantling || isCrouching || xRotation < minClimbAngle || xRotation > maxClimbAngle)
        {
            if (isClimbing) isClimbToggleActive = false;
            if (isClimbing) StopClimbing(true);
            return false;
        }

        RaycastHit hit;
        bool hitClimbable = Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out hit,
            climbDetectionDistance,
            climbableLayer
        );

        // Visual debug for the raycast
        Color rayColor = hitClimbable ? Color.green : Color.red;
        UnityEngine.Debug.DrawRay(playerCamera.position, playerCamera.forward * climbDetectionDistance, rayColor);

        // 1. LATCH ON (Start Climbing)
        if (climbPressed && hitClimbable && !isClimbing)
        {
            // NEW: Get and store the ClimbableSurface component and its properties
            currentSurface = hit.collider.GetComponent<ClimbableSurface>();
            currentClimbSpeedMultiplier = (currentSurface != null) ? currentSurface.climbSpeedMultiplier : 1f;

            isClimbToggleActive = true;
            StartClimbing(hit.normal);
            climbPressed = false;
        }
        // 2. UNLATCH (Stop Climbing - Manual Toggle)
        else if (climbPressed && isClimbing)
        {
            isClimbToggleActive = false;
            climbPressed = false;
        }

        return hitClimbable;
    }

    void StartClimbing(Vector3 surfaceNormal)
    {
        isClimbing = true;
        climbNormal = surfaceNormal;
        clingNormal = surfaceNormal;
        velocity = Vector3.zero;
        UnityEngine.Debug.Log("LATCHED ON!");
    }

    // Updated StopClimbing to optionally bypass gravity application
    void StopClimbing(bool applyGravity = true)
    {
        isClimbing = false;

        // NEW: Clear the surface data on exit
        currentSurface = null;
        currentClimbSpeedMultiplier = 1f;

        if (applyGravity)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity = Vector3.zero;
        }
        UnityEngine.Debug.Log(applyGravity ? "Released Climb!" : "Transitioning to Mantle!");
    }

    // Peak-style Curved Surface Climbing (Latch & Slide)
    void PerformClimbing()
    {
        // 1. Calculate movement axes relative to the curved surface.
        Vector3 right = Vector3.Cross(climbNormal, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, climbNormal).normalized;

        // 2. Determine the intended movement vector based on input AND surface restrictions.
        Vector3 moveDirection = Vector3.zero;

        // Apply movement restrictions from the surface properties
        bool allowsHorizontal = currentSurface == null || currentSurface.allowsHorizontalMovement;
        bool allowsUpward = currentSurface == null || currentSurface.allowsUpwardMovement;

        if (allowsHorizontal)
        {
            moveDirection += right * movementInput.x;
        }
        if (allowsUpward)
        {
            moveDirection += up * movementInput.y;
        }

        // Project onto the wall's tangent plane.
        Vector3 desiredMove = Vector3.ProjectOnPlane(moveDirection, climbNormal);

        // 3. Apply movement with the surface's speed multiplier.
        if (moveDirection.magnitude > 0.1f)
        {
            float speed = climbSpeed * currentClimbSpeedMultiplier;
            controller.Move(desiredMove * speed * Time.deltaTime);
        }

        //// 4. Cling Nudge (Fixed displacement to maintain contact) (Has issue with curved Surfaces)
        //const float CLING_DISTANCE = 0.05f;
        //controller.Move(-clingNormal * CLING_DISTANCE);

        //velocity = Vector3.zero;
    }

    // Check for Mantle when moving past the top of a climbable wall.
    bool TryMantleFromClimbExit()
    {
        if (!isClimbing) return false;

        const float MAX_SEARCH_DISTANCE = 3f;
        const float FORWARD_OFFSET = 0.75f;

        Vector3 rayDownStart = transform.position +
                               transform.forward * FORWARD_OFFSET +
                               Vector3.up * (controller.height + 1.0f);

        RaycastHit hitLedge;

        // 1. Ledge Detection Check (Downward Raycast)
        if (!Physics.Raycast(rayDownStart, Vector3.down, out hitLedge, MAX_SEARCH_DISTANCE, mantleLayer))
        {
            UnityEngine.Debug.DrawLine(rayDownStart, rayDownStart + Vector3.down * MAX_SEARCH_DISTANCE, Color.red, 2f);
            UnityEngine.Debug.Log("Mantle: Failed to find a ledge to grab (Raycast Down Miss).");
            return false;
        }

        // 2. Clearance Check (Upward Raycast)
        Vector3 ledgeTop = hitLedge.point;
        if (Physics.Raycast(ledgeTop + Vector3.up * 0.1f, Vector3.up, standHeight, mantleLayer))
        {
            UnityEngine.Debug.Log("Mantle: Space above ledge is blocked.");
            return false;
        }

        // 3. Ledge Height Check (Check against player's max mantle ability)
        float ledgeHeightFromGround = hitLedge.point.y - transform.position.y;
        if (ledgeHeightFromGround > mantleMaxHeight + 0.1f)
        {
            UnityEngine.Debug.Log($"Mantle: Ledge too high! ({ledgeHeightFromGround:F2}m) Max is {mantleMaxHeight:F2}m.");
            return false;
        }

        // --- SUCCESS LOGIC ---
        UnityEngine.Debug.Log("Mantle: SUCCESS! Pulling up.");

        StopClimbing(false);

        const float FORWARD_CLEARANCE_MULTIPLIER = 1.10f;
        Vector3 finalLedgePosition = hitLedge.point;
        finalLedgePosition += transform.forward * controller.radius * FORWARD_CLEARANCE_MULTIPLIER;

        Vector3 targetPosition = new Vector3(
            finalLedgePosition.x,
            Mathf.Max(hitLedge.point.y, transform.position.y) + controller.height / 2f + 0.01f,
            finalLedgePosition.z
        );

        StartCoroutine(PerformMantle(targetPosition));
        return true;
    }

    IEnumerator PerformMantle(Vector3 targetPosition)
    {
        isMantling = true;
        isClimbToggleActive = false;
        velocity = Vector3.zero;
        float startTime = Time.time;
        Vector3 startPosition = transform.position;

        controller.enabled = false;

        while (Time.time < startTime + mantleDuration)
        {
            float t = (Time.time - startTime) / mantleDuration;
            float smoothT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);

            yield return null;
        }

        // Final Snap, Re-enable Controller, and Grounding
        transform.position = targetPosition;
        controller.enabled = true;
        isMantling = false;
        velocity.y = -2f;
        jumpPressed = false;
    }
}