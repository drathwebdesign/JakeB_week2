using System.Collections;
using UnityEngine;

public class KnockableRespawn : MonoBehaviour {
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isKnockedDown = false;
    private float respawnTime = 30f;
    private float knockdownThreshold = 0.5f; // Angle or other criteria to consider knocked down

    void Start() {
        // Save the original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update() {
        // Check if the lamppost is knocked down by checking its tilt angle or position
        if (!isKnockedDown && IsKnockedDown()) {
            isKnockedDown = true;
            StartCoroutine(RespawnLamppost());
        }
    }

    bool IsKnockedDown() {
        // Check the tilt angle or position to determine if it's knocked down
        float tiltAngle = Quaternion.Angle(originalRotation, transform.rotation);
        return tiltAngle > knockdownThreshold;
    }

    IEnumerator RespawnLamppost() {
        // Wait for the specified respawn time
        yield return new WaitForSeconds(respawnTime);

        // Reset the lamppost's position and rotation
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Reset the Rigidbody state if it exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            // Temporarily disable kinematic state to reset velocity
            rb.isKinematic = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }

        // Mark the lamppost as not knocked down
        isKnockedDown = false;
    }
}

