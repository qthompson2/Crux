using UnityEngine;

public class RadioController : MonoBehaviour
{
    [SerializeField] private Transform player;
    private float distanceToPlayer;

    private bool radioOn = true;
    [SerializeField] private AudioClip radioStaticStartClip;
    private AudioSource radioStaticStartSource;
    private bool hasPlayedStartSound = false;

    [SerializeField] private AudioClip radioStaticClip;
    private AudioSource radioStaticSource;

    [SerializeField] private float staticDistanceThreshold = 10f;
    [SerializeField] private float maxVolumeDistance = 130f;

    [SerializeField] private AudioClip radioMusicIntroClip;
    private AudioSource radioMusicIntroSource;
    
    void Start()
    {
        // Play radio static start sound once.
        radioStaticStartSource = gameObject.AddComponent<AudioSource>();
        radioStaticStartSource.clip = radioStaticStartClip;  
        radioStaticStartSource.playOnAwake = true;
        radioStaticStartSource.spatialBlend = 1f; 
        radioStaticStartSource.minDistance = staticDistanceThreshold;
        radioStaticStartSource.maxDistance = maxVolumeDistance;

        // Setup looping radio static sound.
        radioStaticSource = gameObject.AddComponent<AudioSource>();
        radioStaticSource.clip = radioStaticClip;
        radioStaticSource.loop = true;
        radioStaticSource.playOnAwake = false;
        radioStaticSource.spatialBlend = 1f;
        radioStaticSource.minDistance = staticDistanceThreshold;
        radioStaticSource.maxDistance = maxVolumeDistance;

        // Setup radio music intro sound.
        radioMusicIntroSource = gameObject.AddComponent<AudioSource>();
        radioMusicIntroSource.clip = radioMusicIntroClip;
        radioMusicIntroSource.playOnAwake = false;
        radioMusicIntroSource.spatialBlend = 1f;
        radioMusicIntroSource.minDistance = 1f;
        radioMusicIntroSource.maxDistance = maxVolumeDistance;
    }

    // Update is called once per frame
    void StartStaticSound()
    {
        radioStaticStartSource.volume = 1 - (Mathf.Clamp01((distanceToPlayer - staticDistanceThreshold) / (maxVolumeDistance - staticDistanceThreshold)));
        radioStaticStartSource.Play();
        hasPlayedStartSound = true;
    }

    void StartAnnouncement() 
    {
        Debug.Log("End");
        radioOn = false;
    }

    void TransitionExitRadio()
    {
        radioStaticSource.volume = 0.3f * (1 - (Mathf.Clamp01((staticDistanceThreshold - distanceToPlayer) / staticDistanceThreshold)));
        radioMusicIntroSource.volume =  1 - (Mathf.Clamp01((staticDistanceThreshold - distanceToPlayer) / staticDistanceThreshold));
        if (!radioMusicIntroSource.isPlaying)
        {
            StartAnnouncement();
        }
    }

    void Update()
    {
        if (!radioOn) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!hasPlayedStartSound)
        {
            StartStaticSound();
            return;
        }
        if (radioStaticStartSource.isPlaying)
        {
            radioStaticStartSource.volume = 1 - (Mathf.Clamp01((distanceToPlayer - staticDistanceThreshold) / (maxVolumeDistance - staticDistanceThreshold)));
            return;
        }
        if (distanceToPlayer > staticDistanceThreshold)
        {
            if (!radioStaticSource.isPlaying)
            {
                radioStaticSource.Play();
            }
            radioStaticSource.volume = 1 - (Mathf.Clamp01((distanceToPlayer - staticDistanceThreshold) / (maxVolumeDistance - staticDistanceThreshold)));
        }
        else
        {
            TransitionExitRadio();
            radioMusicIntroSource.Play();
        }
    }
}
