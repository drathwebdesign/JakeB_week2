using UnityEngine;
using System.Collections;

public class ZombieAudioManager : MonoBehaviour {
    public AudioClip[] zombieSounds;  // Array of zombie audio clips
    public float soundDistanceThreshold = 10f;  // Distance threshold for playing sound
    private AudioSource audioSource;
    private Transform playerTransform;  // Reference to the player's transform

    void Start() {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null) {
            return;
        }

        if (zombieSounds == null || zombieSounds.Length == 0) {
            return;
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null) {
            return;
        }

        // Start the coroutine to continuously play zombie sounds
        StartCoroutine(PlayZombieSoundsLoop());
    }

    private IEnumerator PlayZombieSoundsLoop() {
        while (true) {
            PlayRandomZombieSound();

            // Ensure there is a valid clip before waiting
            if (audioSource.clip != null) {
                yield return new WaitForSeconds(audioSource.clip.length);
            } else {
                // If no clip is available, wait for a short time before retrying
                yield return new WaitForSeconds(1f);
            }
        }
    }

    public void PlayRandomZombieSound() {
        if (zombieSounds.Length == 0 || audioSource == null || playerTransform == null) return;

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Play sound only if within the specified distance
        if (distanceToPlayer <= soundDistanceThreshold) {
            int randomIndex = Random.Range(0, zombieSounds.Length);
            AudioClip randomClip = zombieSounds[randomIndex];

            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }
}
