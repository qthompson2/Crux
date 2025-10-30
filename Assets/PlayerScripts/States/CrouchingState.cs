using UnityEngine;

public class CrouchingState : BaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        player.controller.SetHeight(player.controller.crouchHeightMultiplier);
    }

    public override void UpdateState(PlayerStateManager player)
    {
        var input = player.inputHandler.MoveInput;

        // ---- Handle Transitions ----
        if (!player.inputHandler.CrouchHeld)
        {
            if (input.magnitude > 0.1f)
                player.SwitchState(player.walkingState);
            else
                player.SwitchState(player.idleState);

            return;
        }

        if (player.inputHandler.SprintHeld)
        {
            player.SwitchState(player.slidingState);
            return;
        }

        if (player.inputHandler.JumpPressed && player.controller.IsGrounded)
        {
            player.controller.Jump();
            player.SwitchState(player.jumpingState);
            return;
        }

        if (!player.controller.IsGrounded)
        {
            player.SwitchState(player.fallingState);
            return;
        }

        if (player.inputHandler.ClimbHeld && player.controller.CanClimb)
        {
            player.SwitchState(player.climbingState);
            return;
        }

        Vector3 move = player.transform.right * input.x + player.transform.forward * input.y;
        player.controller.Move(move);
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.controller.SetHeight(1.0f);
    }
}
