using UnityEngine;

public class MantlingState : PlayerBaseState
{
	public override void EnterState(PlayerStateManager player)
	{
		player.controller.jumpForce *= 0.4f;
        player.controller.Jump();
        player.controller.jumpForce /= 0.4f;
	}
	
    public override void UpdateState(PlayerStateManager player)
    {
		// ---- Handle Transitions ----
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
