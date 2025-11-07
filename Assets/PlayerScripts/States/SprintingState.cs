using UnityEngine;

public class SprintingState : PlayerBaseState
{
    public override void UpdateState(PlayerStateManager player)
    {
        var input = player.inputHandler.MoveInput;
        player.staminaManager.DrainOverTime(player.staminaManager.sprintCost);

        // ---- Handle Transitions ----

        // Stamina Check
        if (!player.staminaManager.HasStamina()) {
            if (input.magnitude > 0.1f)
                player.SwitchState(player.walkingState);
            else
                player.SwitchState(player.idleState);
            return;
        }

        if (!player.inputHandler.SprintHeld)
        {
            if (input.magnitude > 0.1f)
                player.SwitchState(player.walkingState);
            else
                player.SwitchState(player.idleState);
            return;
        }

        if (player.inputHandler.CrouchHeld)
        {
            player.SwitchState(player.slidingState);
            return;
        }

        if (player.inputHandler.JumpPressed && player.controller.IsGrounded && player.staminaManager.LabourousActionAllowed())
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

        if (player.inputHandler.ClimbHeld && player.controller.CanClimb && player.staminaManager.LabourousActionAllowed())
        {
            player.SwitchState(player.climbingState);
            return;
        }

        Vector3 move = player.transform.right * input.x + player.transform.forward * input.y;
        player.controller.Move(move);
    }
}
