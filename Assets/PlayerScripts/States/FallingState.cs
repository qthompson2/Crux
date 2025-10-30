using System;
using UnityEngine;

public class FallingState : BaseState
{
    public override void UpdateState(PlayerStateManager player)
    {
        float checkDistance = player.controller.groundDetectionDistance;
        float lateralOffset = player.controller.lateralOffset;
        float sphereRadius = player.controller.sphereRadius;
        int requiredValidHits = player.controller.requiredValidHits;
        float maxWalkableAngle = player.controller.maxWalkableAngle;
        float lateralCheckDistance = player.controller.lateralCheckDistance;
        float unstuckSpeed = player.controller.unstuckSpeed;

        var input = player.inputHandler.MoveInput;
        CharacterController cc = player.controller.GetComponent<CharacterController>();
        Transform t = player.transform;

        Vector3 bottomCenter = player.controller.GetCapsuleBottomWorld();

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
            Vector3 origin = bottomCenter + t.TransformDirection(lo);
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
                player.controller.Move(unstuckMove);
                return;
            }
        }

        Vector3 move = t.right * input.x + t.forward * input.y;
        player.controller.Move(move);

        if (player.inputHandler.ClimbHeld && player.controller.CanClimb)
        {
            player.SwitchState(player.climbingState);
            return;
        }
    }

    private void AlignPlayerParallelToSurface(Transform t, Vector3 surfaceNormal, float smoothSpeed)
    {
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.FromToRotation(Vector3.forward, surfaceNormal), Time.deltaTime * smoothSpeed);
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.controller.ResetRotation();
    }
}
