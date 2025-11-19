using UnityEngine;

public class FallingState : PlayerBaseState
{
    private FallProbe probe;

    public override void EnterState(PlayerStateManager player)
    {
        probe = player.controller.probe;

        // Position probe right below player's feet
        Vector3 startPos = player.transform.position - Vector3.up * 0.5f;
        probe.Activate(startPos);

        // stamina tracking
        StaminaManager staminaManager = player.staminaManager;
        if (staminaManager != null)
        { 
            staminaManager.start_height = player.transform.position.y;
            staminaManager.start_time = Time.time;
        }
    }

    public override void UpdateState(PlayerStateManager player)
    {
        player.staminaManager.DrainOverTime(player.staminaManager.staminaRegenRate);

        // 1. PLAYER-BASED GROUNDED CHECK (best for walkable surfaces)
        if (player.controller.IsGrounded)
        {
            player.SwitchState(player.landingState);
            return;
        }

        // 2. PROBE FALLING / SLIDING LOGIC
        probe.lastY = probe.transform.position.y;

        // Pull player toward the probe's position
        Vector3 target = probe.transform.position + Vector3.up * 0.5f;
        Vector3 delta = target - player.transform.position;

        player.controller.Move(delta);

        // 3. PROBE RAYCAST CHECK (detects when probe touches ground)
        if (Physics.Raycast(probe.transform.position, Vector3.down, out RaycastHit hit, 0.6f, player.controller.groundMask))
        {
            player.SwitchState(player.landingState);
            return;
        }

        // 4. PROBE VELOCITY CHECK (detects end of fall or sliding plateau)
        if (Mathf.Abs(probe.rb.linearVelocity.y) < 0.01f)
        {
            player.SwitchState(player.landingState);
            return;
        }

        // 5. OPTIONAL AIR CONTROL
        Vector3 airMove =
            player.transform.right * player.inputHandler.MoveInput.x +
            player.transform.forward * player.inputHandler.MoveInput.y;

        player.controller.Move(airMove * player.controller.airControlSpeed * Time.deltaTime);
    }


    public override void ExitState(PlayerStateManager player)
    {
        probe.Deactivate();
        player.controller.ResetRotation();
    }
}
