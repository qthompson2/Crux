using UnityEngine;

public class MantlingState : BaseState
{
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float mantleDuration = 0.5f;
    private float timer;

    public override void EnterState(PlayerStateManager player)
    {
        startPosition = player.transform.position;
        endPosition = player.controller.GetMantleTargetPosition();

        timer = 0f;
    }

    public override void UpdateState(PlayerStateManager player)
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / mantleDuration);

        player.transform.position = Vector3.Lerp(startPosition, endPosition, t);

        Quaternion targetRotation = Quaternion.LookRotation(player.controller.GetMantleForward());
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 10f);

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
