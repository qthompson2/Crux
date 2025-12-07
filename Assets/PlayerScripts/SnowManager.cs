using UnityEngine;

public class SnowManager : MonoBehaviour
{
    [Tooltip("Snow particle system")]
    public ParticleSystem snowParticleSystem;
    [SerializeField] private float snowStartY = 150f;

    void Update()
    {
        if (transform.position.y >= snowStartY)
        {
            if (!snowParticleSystem.isPlaying)
            {
                snowParticleSystem.Play();
            }
        }
        else
        {
            if (snowParticleSystem.isPlaying)
            {
                snowParticleSystem.Stop();
            }
        }
    }
    
}
