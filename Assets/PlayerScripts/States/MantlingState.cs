using UnityEngine;

public class MantlingState : BaseState
{
    public override void UpdateState(PlayerStateManager player)
    {
        // Mantle Logic

        // ---- Handle Transitions ----
        if (t >= 1f)
        {
            Vector2 input = player.inputHandler.MoveInput;
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
}
