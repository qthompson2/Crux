using UnityEngine;

public class IdleState : BaseState
{

    public override void EnterState(PlayerStateManager player)
    {
        player.UpdateAnimator(this.GetType().Name, true);
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.UpdateAnimator(this.GetType().Name, false);
    }
    public override void UpdateState(PlayerStateManager player)
    {

        var input = player.inputHandler.MoveInput;

       

        // ---- Handle Transitions ----
        if (input.magnitude > 0.1f)
        {
            player.SwitchState(player.walkingState);
            return;
        }

        if (player.inputHandler.SprintHeld)
        {
            player.SwitchState(player.sprintingState);
            return;
        }

        if (player.inputHandler.CrouchHeld)
        {
            player.SwitchState(player.crouchingState);
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
}
