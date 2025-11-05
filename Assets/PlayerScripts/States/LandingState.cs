using UnityEngine;

public class LandingState : PlayerBaseState
{
    public override void UpdateState(PlayerStateManager player)
    {
        Vector2 input = player.inputHandler.MoveInput;
        
        // ---- Handle Transitions ----
        if (input.magnitude > 0.1f)
        {
            if (player.inputHandler.SprintHeld)
                player.SwitchState(player.sprintingState);
            else if (player.inputHandler.CrouchHeld)
                player.SwitchState(player.crouchingState);
            else
                player.SwitchState(player.walkingState);
        }
        else
        {
            player.SwitchState(player.idleState);
        }
    }
}
