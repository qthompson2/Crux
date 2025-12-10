using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlareGun : ItemClass
{
    [Header("Gun Setup")]
    [SerializeField] private Rigidbody flareProjectile;
    [SerializeField] private float launchForce = 400f;

    [Header("Noise Settings (The Bang)")]
    [SerializeField] private float noiseRadius = 25f; // Much larger than the flare light
    [SerializeField] private List<string> scareTags;

    [Header("Spawn Settings")]
    [SerializeField] private float originOffset = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip emptySound;

    private AudioSource audioSource;
    private bool hasAmmo = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        destroyOnUse = false;
    }

    public override void Use()
    {
        if (hasAmmo) Fire();
        else DryFire();
    }

    private void Fire()
    {
        hasAmmo = false;

        // 1. VISUALS & AUDIO
        if (fireSound) audioSource.PlayOneShot(fireSound);

        // 2. PHYSICS: Spawn the physical flare (The Light)
        if (flareProjectile && playerCamera)
        {
            Vector3 spawnPos = playerCamera.transform.position + (playerCamera.transform.forward * originOffset);
            Quaternion spawnRot = playerCamera.transform.rotation;

            Rigidbody bullet = Instantiate(flareProjectile, spawnPos, spawnRot);
            bullet.AddForce(playerCamera.transform.forward * launchForce);
        }

        // 3. LOGIC: Create the Noise (The Sound)
        // This happens immediately at the player's position
        CreateNoise();

        Debug.Log("Flare Gun fired!");
    }

    private void DryFire()
    {
        if (emptySound) audioSource.PlayOneShot(emptySound);
    }

    // --- NEW NOISE LOGIC ---
    private void CreateNoise()
    {
        // Find all colliders within the "Bang" radius around the player/gun
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, noiseRadius);

        foreach (var hitCollider in hitColliders)
        {
            // "Does the list of scare tags contain this object's tag?"
            if (scareTags.Contains(hitCollider.tag))
            {
                ScareMonster(hitCollider.gameObject);
            }
        }
    }

    private void ScareMonster(GameObject monster)
    {
        Debug.Log($"BANG! Scared monster at player location: {monster.name}");

        // TODO: Hook up your AI script here to scare

    }

    // Visual debugging to see the noise range in the Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // Red for "Noise/Danger"
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}