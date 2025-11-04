using UnityEngine;

public class SlidingState : BaseState
{
    private float slideTimer;

    public override void EnterState(PlayerStateManager player)
    {
        slideTimer = 0f;
        player.controller.SetHeight(player.controller.slideHeightMultiplier);
    }

    public override void UpdateState(PlayerStateManager player)
    {
        slideTimer += Time.deltaTime;

        Vector2 input = player.inputHandler.MoveInput;
        Vector3 move = player.transform.forward + player.transform.right * input.x * 0.5f;
        player.controller.Move(move);

        // ---- Handle Transitions ----
        if (!player.controller.IsGrounded)
        {
            player.SwitchState(player.fallingState);
            return;
        }

        if (slideTimer >= player.controller.slideDuration || !player.inputHandler.CrouchHeld)
        {
            if (input.magnitude > 0.1f)
                player.SwitchState(player.crouchingState);
            else
                player.SwitchState(player.idleState);
        }

        if (player.inputHandler.JumpPressed && player.controller.IsGrounded && player.staminaManager.LabourousActionAllowed())
        {
            player.controller.Jump();
            player.SwitchState(player.jumpingState);
            return;
        }

        if (player.inputHandler.ClimbHeld && player.controller.CanClimb && player.staminaManager.LabourousActionAllowed())
        {
            player.SwitchState(player.climbingState);
            return;
        }
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.controller.SetHeight(1.0f);
    }
}
