using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour {
    [SerializeField] float walkSpeed = 2.0f;
    [SerializeField] float runSpeed = 4.0f;
    [SerializeField] float directionChangeInterval = 5.0f; // Time interval for changing direction
    [SerializeField] float detectionRange = 20.0f; // Range within which the zombie detects the target

    private Vector3 direction;
    private Transform targetTransform;

    // Animation fields
    private bool isWalking;
    private bool isRunning;

    void Start() {
        // Start changing direction periodically
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update() {
        DetectTarget();

        if (isRunning && targetTransform != null) {
            // Calculate the direction towards the target
            direction = (targetTransform.position - transform.position).normalized;
            // Move towards the target
            transform.Translate(direction * runSpeed * Time.deltaTime, Space.World);
        } else if (isWalking) {
            // Move in the current direction
            transform.Translate(direction * walkSpeed * Time.deltaTime, Space.World);
        }

        // Update the walking state
        isWalking = direction != Vector3.zero && !isRunning;
    }

    IEnumerator ChangeDirectionRoutine() {
        while (true) {
            if (!isRunning) {
                // Wait for the defined interval
                yield return new WaitForSeconds(directionChangeInterval);

                // Pick a new random direction
                direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            } else {
                yield return null; // Continue checking for the target
            }
        }
    }

    void DetectTarget() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hitCollider in hitColliders) {
            if (hitCollider.CompareTag("Hooker")) {
                targetTransform = hitCollider.transform;
                isRunning = true;
                return;
            }
        }
        targetTransform = null;
        isRunning = false;
    }

    public bool IsWalking() {
        return isWalking;
    }

    public bool IsRunning() {
        return isRunning;
    }
}
