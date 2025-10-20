using UnityEngine;

public class FallingState : BaseState
{
    [Header("Ground Detection")]
    public float checkDistance = 0.75f;      // how far down to look for ground
    public float lateralOffset = 0.35f;     // offset from center to cast side checks
    public float sphereRadius = 0.12f;      // radius for spherecasts (safe for small bumps)
    public int requiredValidHits = 3;       // how many valid hits -> landed
    public float maxWalkableAngle = 50f;    // max slope angle considered ground
    public float lateralCheckDistance = 1f; // how far to check for side walls

    [Header("Unstick (when touching non-walkable surfaces)")]
    public float unstuckSpeed = 50f;       // how fast to nudge away from non-walkable surfaces

    public override void UpdateState(PlayerStateManager player)
    {
        var input = player.inputHandler.MoveInput;
        CharacterController cc = player.controller.GetComponent<CharacterController>();
        Transform t = player.transform;

        // compute bottom center of capsule in world space (works with non-default center)
        Vector3 bottomCenter = GetCapsuleBottomWorld(cc, t);

        // local-space offsets (relative to transform)
        Vector3[] localOffsets = new Vector3[]
        {
            Vector3.zero,
            Vector3.forward * lateralOffset,
            -Vector3.forward * lateralOffset,
            Vector3.right * lateralOffset,
            -Vector3.right * lateralOffset
        };

        int validHits = 0;
        int totalHits = 0;
        Vector3 avgValidNormal = Vector3.zero;
        Vector3 avgContactNormal = Vector3.zero;

        foreach (var lo in localOffsets)
        {
            Vector3 origin = bottomCenter + t.TransformDirection(lo); // local -> world
            // use a short downward spherecast to handle small irregularities
            if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, checkDistance, player.controller.groundMask, QueryTriggerInteraction.Ignore))
            {
                totalHits++;
                avgContactNormal += hit.normal;

                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle <= maxWalkableAngle)
                {
                    validHits++;
                    avgValidNormal += hit.normal;
                    Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.green);
                }
                else
                {
                    Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.yellow);
                }
            }
            else
            {
                Debug.DrawRay(origin, Vector3.down * checkDistance, Color.red);
            }
        }

        if (validHits >= requiredValidHits) 
        { 
            player.SwitchState(player.landingState);
            return;
        }

        bool sideWallDetected = false;
        Vector3 avgSideNormal = Vector3.zero;

        // Only check sides if we didn't detect enough valid hits
        if (validHits < requiredValidHits)
        {
            Vector3[] sideDirs = new Vector3[]
            {
                t.right,
                -t.right,
                t.forward,
                -t.forward
            };

            foreach (var dir in sideDirs)
            {
                Vector3 origin = bottomCenter;
                if (Physics.SphereCast(origin, sphereRadius, dir, out RaycastHit hit, lateralCheckDistance, player.controller.groundMask, QueryTriggerInteraction.Ignore))
                {
                    sideWallDetected = true;
                    avgSideNormal += hit.normal;
                    Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.blue);
                }
                else
                {
                    Debug.DrawRay(origin, dir * lateralCheckDistance, Color.red);
                }
            }

            if (sideWallDetected)
            {
                avgSideNormal.Normalize();
                AlignPlayerParallelToSurface(t, avgSideNormal, player.controller.rotationSmoothSpeed);
                Vector3 horizontalNormal = Vector3.ProjectOnPlane(avgSideNormal, Vector3.up).normalized;
                Vector3 unstuckMove = horizontalNormal * unstuckSpeed * Time.deltaTime;

                Debug.DrawRay(t.position, -avgSideNormal * 2f, Color.cyan);
                Debug.DrawRay(t.position, avgSideNormal * 2f, Color.green);
                Debug.DrawRay(t.position, horizontalNormal * 2f, Color.magenta);

                player.controller.Move(unstuckMove);
                return;
            }
        }

        // --- Then fallback to normal movement if no wall detected ---
        Vector3 move = t.right * input.x + t.forward * input.y;
        player.controller.Move(move);
    }

    // compute world-space bottom center of CharacterController capsule
    private Vector3 GetCapsuleBottomWorld(CharacterController cc, Transform t)
    {
        // CharacterController.center is local-space. bottom = transform.position + center - height/2 + radius
        Vector3 centerWorld = t.position + t.TransformVector(cc.center);
        float halfHeight = cc.height * 0.5f;
        float bottomY = centerWorld.y - halfHeight + cc.radius;
        return new Vector3(centerWorld.x, bottomY, centerWorld.z);
    }

    private void AlignPlayerParallelToSurface(Transform t, Vector3 surfaceNormal, float smoothSpeed)
    {
        // Calculate the new "up" direction — parallel to the surface plane
        // To be parallel to the surface, our up vector should be perpendicular to the surface normal
        Vector3 newUp = Vector3.Cross(t.right, surfaceNormal).normalized;

        // Then recalculate forward based on that new up and the wall normal
        Vector3 newForward = Vector3.Cross(newUp, surfaceNormal).normalized;

        // Build rotation that aligns player up to the newUp direction
        Quaternion targetRotation = Quaternion.LookRotation(newForward, newUp);

        // Smoothly rotate
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.controller.ResetRotation();
    }
}
