using UnityEngine;

public class JumpingState : BaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        if (player.controller.IsGrounded)
        {
            player.controller.Jump();
        }
    }

    public override void UpdateState(PlayerStateManager player)
    {
        Vector2 input = player.inputHandler.MoveInput;
        Vector3 move = player.transform.right * input.x + player.transform.forward * input.y;
        player.controller.Move(move);
        player.controller.Jump();

        // ---- Handle Transitions ----
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
    }
}
