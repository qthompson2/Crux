using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class FlareProjectile : MonoBehaviour
{
    [Header("Flare Stats")]
    [SerializeField] private float burnTime = 60f;
    [SerializeField] private float lightIntensity = 2f;
    [SerializeField] private Color burnColor = Color.red;

    [Header("Visuals (Flicker)")]
    [SerializeField] private Transform radiusVisual;
    [SerializeField] private float flickerSpeed = 5f;
    [SerializeField] private float flickerStrength = 0.5f;
    [SerializeField] private float haloShake = 0.05f;

    [Header("Collision")]
    [SerializeField] private LayerMask stickableLayers;
    [SerializeField] private Collider myCollider;

    [Header("AI Interaction")]
    [SerializeField] private float scareRadius = 10f;
    [SerializeField] private float checkInterval = 0.5f;

    
    [SerializeField] private List<string> scareTags;

    // Private State
    private Light flareLight;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isBurning = true;
    private bool hasStuck = false;
    private Vector3 baseScale;

    private void Awake()
    {
        flareLight = GetComponent<Light>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (myCollider == null) myCollider = GetComponent<Collider>();

        // SETUP VISUALS
        float diameter = scareRadius * 2f;
        baseScale = new Vector3(diameter, diameter, diameter);

        if (radiusVisual != null)
        {
            radiusVisual.localScale = baseScale;
            radiusVisual.gameObject.SetActive(false);
        }

        // SETUP LIGHT
        flareLight.color = burnColor;
        flareLight.intensity = lightIntensity;
        flareLight.range = scareRadius * 1.5f;

        // SETUP PHYSICS
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        StartCoroutine(BurnRoutine());
        StartCoroutine(MonsterCheckRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasStuck) return;

        if ((stickableLayers.value & (1 << collision.gameObject.layer)) > 0)
        {
            StickToSurface(collision);
        }
    }

    private void StickToSurface(Collision collision)
    {
        hasStuck = true;

        // --- FREEZE PHYSICS ---
        rb.isKinematic = true;

        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // --- DISABLE COLLIDER ---
        myCollider.enabled = false;

        // --- PARENT TO SURFACE ---
        transform.SetParent(collision.transform);

        // --- SHOW VISUALS ---
        if (radiusVisual != null)
        {
            radiusVisual.gameObject.SetActive(true);
        }

        // --- PLAY AUDIO ---
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private IEnumerator BurnRoutine()
    {
        float elapsedBurn = 0f;
        float randomOffset = Random.Range(0f, 100f);

        while (elapsedBurn < burnTime)
        {
            elapsedBurn += Time.deltaTime;

            // --- FLICKER LOGIC ---
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);

            // Flicker Light
            flareLight.intensity = lightIntensity + (noise * flickerStrength);

            // Shake Sphere Size
            if (radiusVisual != null)
            {
                float shakeMultiplier = 1f + (noise * haloShake);
                radiusVisual.localScale = baseScale * shakeMultiplier;
            }

            yield return null;
        }

        // --- FADE OUT LOGIC ---
        isBurning = false;
        float fadeDuration = 1f;
        float currentIntensity = flareLight.intensity;
        float currentVolume = audioSource.volume;
        Vector3 currentScale = (radiusVisual != null) ? radiusVisual.localScale : Vector3.zero;

        float fadeTimer = 0f;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float percent = fadeTimer / fadeDuration;

            flareLight.intensity = Mathf.Lerp(currentIntensity, 0f, percent);
            audioSource.volume = Mathf.Lerp(currentVolume, 0f, percent);

            if (radiusVisual != null)
            {
                radiusVisual.localScale = Vector3.Lerp(currentScale, Vector3.zero, percent);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator MonsterCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (isBurning)
        {
            DetectMonsters();
            yield return wait;
        }
    }

    private void DetectMonsters()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, scareRadius);
        foreach (var hitCollider in hitColliders)
        {
            
            if (scareTags.Contains(hitCollider.tag))
            {
                ScareMonster(hitCollider.gameObject);
            }
        }
    }

    private void ScareMonster(GameObject monster)
    {
        // Debug.Log($"Scaring monster: {monster.name}");
        // TODO: Hook up your AI script here
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scareRadius);
    }
}