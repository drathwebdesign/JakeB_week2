using UnityEngine;

public class ZombieAudioManager : MonoBehaviour {
    public AudioClip[] zombieSounds;  // Array of zombie audio clips
    private AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null) {
            Debug.LogError("AudioSource component missing from this GameObject!", gameObject);
            return;
        }

        if (zombieSounds == null || zombieSounds.Length == 0) {
            Debug.LogWarning("No audio clips assigned to ZombieAudioManager.", gameObject);
            return;
        }

        PlayRandomZombieSound();  // Play sound at start for testing
    }

    public void PlayRandomZombieSound() {
        if (zombieSounds.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, zombieSounds.Length);
        AudioClip randomClip = zombieSounds[randomIndex];

        audioSource.clip = randomClip;
        Debug.Log("Playing clip: " + randomClip.name);  // Log the clip name for debugging
        audioSource.Play();
    }
}
