using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientSoundManager : MonoBehaviour
{
    [Header("Ambient Clips")]
    public List<AudioClip> ambientClips = new List<AudioClip>();

    [Header("Silence Delay (seconds)")]
    public float minSilenceTime = 2f;
    public float maxSilenceTime = 8f;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Fade Settings (seconds)")]
    public float fadeInTime = 2f;
    public float fadeOutTime = 2f;


    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Start silent (for fade in)
        audioSource.volume = 0f;

        StartCoroutine(AmbientLoop());
    }

    private IEnumerator AmbientLoop()
    {
        while (true)
        {
            if (ambientClips.Count == 0)
            {
                Debug.LogWarning("AmbientSoundManager: No clips assigned!");
                yield break;
            }

            // Pick a random clip
            AudioClip clip = ambientClips[Random.Range(0, ambientClips.Count)];

            audioSource.clip = clip;
            audioSource.Play();

            // Fade in
            yield return StartCoroutine(FadeIn(audioSource, fadeInTime));

            // Wait for the rest of the clip duration *after* fade-in
            float remaining = clip.length - fadeInTime;
            if (remaining > 0)
                yield return new WaitForSeconds(remaining);

            // Fade out
            yield return StartCoroutine(FadeOut(audioSource, fadeOutTime));

            // Random silence time
            float silence = Random.Range(minSilenceTime, maxSilenceTime);
            yield return new WaitForSeconds(silence);
        }
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        source.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }

        source.volume = 0f;
    }
}
