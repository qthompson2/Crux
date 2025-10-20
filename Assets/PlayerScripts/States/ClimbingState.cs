using UnityEngine;

public class ClimbingState : BaseState
{
    public override void UpdateState(PlayerStateManager player)
    {
        float sampleDistance = player.controller.sampleDistance;
        float stickDistance = player.controller.stickDistance;
        float stickLerp = player.controller.stickLerp;
        if (!player.inputHandler.ClimbHeld)
        {
            player.SwitchState(player.fallingState);
            return;
        }

        // 1) Try to get an up-to-date wall normal (prefer controller.GetWallNormal() if reliable)
        Vector3 wallNormal = player.controller.GetWallNormal();

        // If that normal seems invalid (zero) or you want more robust sampling, do a short ray/sphere cast forward:
        if (wallNormal == Vector3.zero)
        {
            RaycastHit hit;
            Vector3 origin = player.transform.position + Vector3.up * 0.5f; // chest height
            if (Physics.SphereCast(origin, 0.25f, player.transform.forward, out hit, sampleDistance))
            {
                wallNormal = hit.normal;
            }
            else
            {
                // fallback: give up and fall
                player.SwitchState(player.fallingState);
                return;
            }
        }

        // 2) Build a proper wall-frame
        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
        if (wallUp.sqrMagnitude < 0.001f) wallUp = Vector3.up; // safety
        Vector3 wallRight = Vector3.Cross(wallNormal, wallUp).normalized;

        // 3) Read input and form move vector (same pattern as your working code)
        Vector2 input = player.inputHandler.MoveInput;
        Vector3 move = wallRight * input.x + wallUp * input.y;

        // IMPORTANT: Remove ANY normal component so CC doesn't treat it as a penetration & cancel sideways motion
        move = Vector3.ProjectOnPlane(move, wallNormal);

        // Debug
        Debug.DrawRay(player.transform.position, move, Color.cyan);
        // Debug.Log($"Climb move (proj): {move}, input: {input}, wallNormal: {wallNormal}");

        // 4) Try to gently keep player at a fixed distance from wall (prevent drifting off curved walls)
        // Compute desired point on wall using the hit from SphereCast if available; otherwise estimate using wallNormal.
        // We'll compute a small perpendicular correction that does not cancel input, just nudges toward the wall.
        Vector3 correction = Vector3.zero;
        {
            // Prefer using a recent hit point if available - do a short raycast to find contact point
            RaycastHit hit;
            Vector3 origin = player.transform.position + Vector3.up * 0.5f;
            if (Physics.SphereCast(origin, 0.25f, player.transform.forward, out hit, sampleDistance))
            {
                // desired position is on the wall at (hit.point + normal * (characterRadius + stickDistance))
                CharacterController cc = player.controller.GetComponent<CharacterController>();
                float radius = (cc != null) ? cc.radius : 0.5f;
                Vector3 desired = hit.point + hit.normal * (radius + stickDistance);
                Vector3 toDesired = desired - player.transform.position;

                // keep only perpendicular part (do not cancel movement along the wall)
                correction = Vector3.Project(toDesired, -hit.normal);

                // smooth the correction (apply a fraction right away so we don't snap)
                correction = correction * Mathf.Clamp01(stickLerp * Time.deltaTime);
                // ensure correction does not contain a component along the wall (safety)
                correction = Vector3.ProjectOnPlane(correction, hit.normal);
            }
        }

        // 5) Combine the intended direction and the small correction, then send to controller
        Vector3 combined = move + correction;

        // NOTE: your controller.Move expects a direction (it multiplies by speed & deltaTime).
        // If your Move expects an absolute displacement this will need to be scaled: combined * speed * Time.deltaTime
        player.controller.Move(combined);

        // 6) Rotate to face wall as before
        // Quaternion targetRotation = Quaternion.LookRotation(-wallNormal, wallUp);
        // player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 10f);

        // 7) Mantle check
        if (player.controller.CanMantle)
        {
            player.SwitchState(player.mantlingState);
            return;
        }
    }
}
