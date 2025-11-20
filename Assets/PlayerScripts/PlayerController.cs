using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header ("State Manager")]
    [SerializeField] public PlayerStateManager playerStateManager;

    [Header("Move Settings")]
    [SerializeField] public float walkSpeed = 5.0f;
    [SerializeField] public float sprintSpeedMultiplier = 3f;
    [SerializeField] public float jumpForce = 1.5f;
    public Vector3 StoredJumpMomentum;
    public Vector3 LastGroundedMove;
    private Vector3 velocity;

    [Header ("Climbing Settings")]
    [SerializeField] public float gravity = -9.81f;
    [SerializeField] public float climbSpeedMultiplier = 0.6f;
    [SerializeField] public float maxClimbingRotationAngle = 60f;
    [SerializeField] public float climbDetectionDistance = 1.5f;
    [SerializeField] public float mantlableSurfaceAngle = 0.9f;
    [SerializeField] public float sampleDistance = 1.0f;
    [SerializeField] public float stickDistance = 0.12f;
    [SerializeField] public float stickLerp = 8f;
    [SerializeField] public float rotationSmoothSpeed = 6f;
    [SerializeField] public LayerMask groundMask;
    private float climbingYawOffset = 0f;
    
    [Header ("Landing Detection Settings")]
    [SerializeField] public float unstuckSpeed = 1000f;
    [SerializeField] public float groundDetectionDistance = 0.75f;
    [SerializeField] public float lateralOffset = 0.35f;
    [SerializeField] public float sphereRadius = 0.12f;
    [SerializeField] public int requiredValidHits = 3;
    [SerializeField] public float maxWalkableAngle = 45f;
    [SerializeField] public float lateralCheckDistance = 1f;
    [SerializeField] public FallProbe probe;
    [SerializeField] public float airControlSpeed = 6.5f;

    [Header ("Character Settings")]
    [SerializeField] public float playerHeight = 2.0f;

    [Header ("Crouch/Slide Settings")]
    [SerializeField] public float crouchHeightMultiplier = 0.5f;
    [SerializeField] public float slideHeightMultiplier = 0.4f;
    [SerializeField] public float slideSpeedMultiplier = 2.5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] public float slideDuration = 1.0f;

    [Header ("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] public float lookSpeed = 10.0f;
    [SerializeField] public float rotationSmoothTime = 0.2f;
    [SerializeField] public float maxVerticalAngle = 90f;
    private float pitch = 0f;

    public bool IsGrounded
    {
        get
        {
            return CapsuleGroundCheck() || SpherePatternGroundCheck();
        }
    }

    private bool CapsuleGroundCheck()
    {
        CharacterController cc = GetComponent<CharacterController>();
        float radius = cc.radius;
        float height = cc.height;

        // Capsule endpoints
        Vector3 top = transform.position + Vector3.up * (height / 2f - radius);
        Vector3 bottom = transform.position + Vector3.up * radius;

        float distance = groundDetectionDistance;

        if (Physics.CapsuleCast(top, bottom, radius, Vector3.down, out RaycastHit hit, distance, groundMask))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= maxWalkableAngle;
        }

        return false;
    }

    private bool SpherePatternGroundCheck()
    {
        float radius = 0.45f;
        float distance = groundDetectionDistance + 0.1f;

        Vector3 origin = transform.position + Vector3.up * 0.2f;

        // 5-point pattern: center + cardinal directions
        Vector3[] offsets =
        {
            Vector3.zero,
            Vector3.forward * 0.15f,
            Vector3.back * 0.15f,
            Vector3.left * 0.15f,
            Vector3.right * 0.15f
        };

        foreach (var off in offsets)
        {
            Vector3 castOrigin = origin + off;

            if (Physics.SphereCast(castOrigin, radius, Vector3.down, out RaycastHit hit, distance, groundMask))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);

                // If ANY cast is walkable surface → grounded
                if (angle <= maxWalkableAngle)
                    return true;
            }
        }

        return false;
    }



    public bool CanClimb {
        get {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, climbDetectionDistance, groundMask))
            {
                return true;
            }
            return false;
        }
    }

    public bool CanMantle {
        get {
			if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit hit, climbDetectionDistance, groundMask))
			{
				if (Vector3.Dot(hit.normal, Vector3.up) > mantlableSurfaceAngle)
				{
					return true;
				}
			}
			return false;
        }
    }

    public void ResetRotation() {
        StartCoroutine(SmoothResetRotation());
    }

    private IEnumerator SmoothResetRotation() {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < rotationSmoothTime) {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, (elapsedTime / rotationSmoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    public void Move(Vector3 move) {
        CharacterController cc = GetComponent<CharacterController>();

        float speedMultiplier = 1.0f;
        if (playerStateManager.currentState is ClimbingState) speedMultiplier = climbSpeedMultiplier;
        if (playerStateManager.currentState is SprintingState) speedMultiplier = sprintSpeedMultiplier;
        if (playerStateManager.currentState is SlidingState) speedMultiplier = slideSpeedMultiplier;
        if (playerStateManager.currentState is CrouchingState) speedMultiplier = crouchSpeedMultiplier;

        Vector3 finalMove = move * walkSpeed * speedMultiplier;

        // Capture grounded motion for later use in jump
        if (IsGrounded && finalMove.sqrMagnitude > 0.0001f)
            LastGroundedMove = finalMove;   // finalMove already includes speed multipliers

        cc.Move(finalMove * Time.deltaTime);
    }


    public void Jump() {
        if (IsGrounded) {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    public Vector3 GetMantleForward() {
        return Vector3.zero;
    }

    public Vector3 GetWallNormal() {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, climbDetectionDistance, groundMask))
        {
            return hit.normal;
        }
        return Vector3.zero;
    }

    public Vector3 GetMantleTargetPosition() {
        return Vector3.zero;
    }

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
        if (playerStateManager == null)
        {
            playerStateManager = GetComponent<PlayerStateManager>();
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetHeight(float heightMultiplier) {
        
    }

    public void LookUpdate(PlayerStateManager playerStateManager)
    {
        Vector2 lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        pitch -= lookInput.y * lookSpeed;
        pitch = Mathf.Clamp(pitch, -maxVerticalAngle, maxVerticalAngle);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);

        if (playerStateManager.currentState is ClimbingState)
        {
            Vector3 wallNormal = GetWallNormal();
            Vector3 wallForward = -wallNormal;
            Quaternion wallRotation = Quaternion.LookRotation(wallForward, Vector3.up);

            climbingYawOffset += lookInput.x * lookSpeed;
            climbingYawOffset = Mathf.Clamp(climbingYawOffset, -maxClimbingRotationAngle, maxClimbingRotationAngle);

            Quaternion targetRotation = Quaternion.AngleAxis(climbingYawOffset, Vector3.up) * wallRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
        else
        {
            transform.Rotate(Vector3.up, lookInput.x * lookSpeed);
            climbingYawOffset = 0f;
        }
    }

    public void GravityUpdate(PlayerStateManager playerStateManager) {
        if (playerStateManager.currentState is ClimbingState) {
            return;
        }
        if (IsGrounded && velocity.y < 0) {
            velocity.y = -9.81f;
        } else {
            velocity.y += gravity * Time.deltaTime;
        }

        CharacterController cc = GetComponent<CharacterController>();
        cc.Move(velocity * Time.deltaTime);
    }

    public Vector3 GetCapsuleBottomWorld()
    {
        CharacterController cc = GetComponent<CharacterController>();
        Transform t = transform;
        Vector3 centerWorld = t.position + t.TransformVector(cc.center);
        float halfHeight = cc.height * 0.5f;
        float bottomY = centerWorld.y - halfHeight + cc.radius;
        return new Vector3(centerWorld.x, bottomY, centerWorld.z);
    }

    void Update()
    {
        Debug.Log("IsGrounded: " + IsGrounded);
        LookUpdate(playerStateManager);
        GravityUpdate(playerStateManager);
    }
}
