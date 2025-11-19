using UnityEngine;

public class JumpingState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        if (player.controller.IsGrounded)
        {
            // Turn off probe so FallingState reactivates it correctly
            player.controller.probe.Deactivate();

            // Capture current horizontal momentum BEFORE the jump impulse
            player.controller.StoredJumpMomentum = player.controller.LastGroundedMove;

            // Vertical part of jump
            player.controller.Jump();

            // Stamina cost
            player.staminaManager.DrainStamina(player.staminaManager.jumpCost);
        }

        // Immediately switch to Falling
        player.SwitchState(player.fallingState);
    }
}
