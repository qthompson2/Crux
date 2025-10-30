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
        Vector3 wallNormal = player.controller.GetWallNormal();

        // 1) Ensure we have a valid wall normal
        if (wallNormal == Vector3.zero)
        {
            RaycastHit hit;
            Vector3 origin = player.transform.position + Vector3.up * 0.5f;
            if (Physics.SphereCast(origin, 0.25f, player.transform.forward, out hit, sampleDistance))
            {
                wallNormal = hit.normal;
            }
            else
            {
                player.SwitchState(player.fallingState);
                return;
            }
        }

        // 2) Build a proper wall-frame
        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
        if (wallUp.sqrMagnitude < 0.001f) wallUp = Vector3.up;
        Vector3 wallRight = Vector3.Cross(wallNormal, wallUp).normalized;

        // 3) Read input and form move vector (same pattern as your working code)
        Vector2 input = player.inputHandler.MoveInput;
        Vector3 move = wallRight * input.x + wallUp * input.y;
        move = Vector3.ProjectOnPlane(move, wallNormal); // IMPORTANT: Remove ANY normal component so CC doesn't treat it as a penetration & cancel sideways motion

        // 4) Try to gently keep player at a fixed distance from wall (prevent drifting off curved walls)
        Vector3 correction = Vector3.zero;
        {
            RaycastHit hit;
            Vector3 origin = player.transform.position + Vector3.up * 0.5f;
            if (Physics.SphereCast(origin, 0.25f, player.transform.forward, out hit, sampleDistance))
            {
                CharacterController cc = player.controller.GetComponent<CharacterController>();
                float radius = (cc != null) ? cc.radius : 0.5f;
                Vector3 desired = hit.point + hit.normal * (radius + stickDistance);
                Vector3 toDesired = desired - player.transform.position;

                correction = Vector3.Project(toDesired, -hit.normal);
                correction = correction * Mathf.Clamp01(stickLerp * Time.deltaTime);
                correction = Vector3.ProjectOnPlane(correction, hit.normal);
            }
        }

        // 5) Combine the intended direction and the small correction, then send to controller
        Vector3 combined = move + correction;
        player.controller.Move(combined);

        // 6) Mantle check
        if (player.controller.CanMantle)
        {
            player.SwitchState(player.mantlingState);
            return;
        }
    }
}
