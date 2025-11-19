using UnityEngine;

public class JumpingState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        if (player.controller.IsGrounded)
        {
            player.controller.probe.Deactivate();

            // Apply the upward impulse ONCE.
            player.controller.Jump();

            // Drain stamina ONCE.
            player.staminaManager.DrainStamina(player.staminaManager.jumpCost);
        }

        // Immediately transition to falling.
        player.SwitchState(player.fallingState);
    }
}
