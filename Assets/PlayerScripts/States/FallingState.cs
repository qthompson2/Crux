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

        // -------------------------------------------------
        // 1. GATHER ALL EXIT CONDITIONS FIRST
        // -------------------------------------------------

        // A) Player's own grounded check
        bool grounded = player.controller.IsGrounded;

        // B) Probe raycast touching ground
        bool probeHitGround = Physics.Raycast(
            probe.transform.position,
            Vector3.down,
            out RaycastHit hit,
            0.6f,
            player.controller.groundMask
        );

        // C) Probe has stopped falling (hit something or resting)
        bool probeStopped = Mathf.Abs(probe.rb.linearVelocity.y) < 0.01f;

        // D) Ensure player and probe are close vertically (prevents sliding glitches)
        float verticalDelta = Mathf.Abs((probe.transform.position.y + 0.5f) - player.transform.position.y);
        bool closeEnough = verticalDelta < 0.2f;

        // -------------------------------------------------
        // 2. DEFINITIVE LANDING RULE
        // Must meet ONE ground condition AND proximity condition
        // -------------------------------------------------

        // either check if the IsGrounded flag is true, or if the probe has hit the ground, stopped moving
        if (grounded || ((probeHitGround || probeStopped) && closeEnough))
        {
            Debug.Log("EXITING FALLING STATE (definitive)");
            player.SwitchState(player.landingState);
            return;
        }

        if (player.inputHandler.ClimbHeld && player.controller.CanClimb && player.staminaManager.LabourousActionAllowed())
        { 
            player.SwitchState(player.climbingState);
            return;
        }

        // -------------------------------------------------
        // 3. CONTINUE NORMAL FALLING MOTION
        // -------------------------------------------------

        // Update probe vertical tracking
        probe.lastY = probe.transform.position.y;

        // Player follows probe
        Vector3 target = probe.transform.position + Vector3.up * 0.5f;
        Vector3 delta = target - player.transform.position;

        player.controller.Move(delta);


        // -------------------------------------------------
        // 4. AIR CONTROL + preserved momentum
        // -------------------------------------------------

        // Vector3 airMove =
        //     player.transform.right * player.inputHandler.MoveInput.x +
        //     player.transform.forward * player.inputHandler.MoveInput.y;

        // // Combine stored momentum + air steering
        // Vector3 finalAirMove =
        //     player.controller.StoredJumpMomentum +
        //     (airMove * player.controller.airControlSpeed);

        // player.controller.Move(finalAirMove * Time.deltaTime);

        // // Gradually decay the stored momentum
        // player.controller.StoredJumpMomentum = Vector3.Lerp(
        //     player.controller.StoredJumpMomentum,
        //     Vector3.zero,
        //     Time.deltaTime * 2f   // adjust decay speed if needed
        // );

    }



    public override void ExitState(PlayerStateManager player)
    {
        probe.Deactivate();
        player.controller.ResetRotation();
    }
}
