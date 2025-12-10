using UnityEngine;

public class PlayerSoundPlayer : MonoBehaviour
{
    public AudioClip footStepSound;
    public AudioClip climbHandLift;
    public AudioClip climbHandDown;
    public AudioClip jumpingSound;
    public AudioClip landingSound;

    AudioSource playerAudioSource;
    private void Awake()
    {
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
            playerAudioSource.playOnAwake = false;
        }
    }

    public void PlayFootstep()
    {
        playerAudioSource.PlayOneShot(footStepSound);
    }

    public void PlayHandLift()
    {
        playerAudioSource.PlayOneShot(climbHandLift);
    }

    public void PlayHandDown()
    {
        playerAudioSource.PlayOneShot(climbHandDown);
    }

    public void PlayJump()
    {
        playerAudioSource.PlayOneShot(jumpingSound);
    }

    public void PlayLand()
    {
        playerAudioSource.PlayOneShot(landingSound);
    }

}