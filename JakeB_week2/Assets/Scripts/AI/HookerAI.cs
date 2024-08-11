using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HookerAI : MonoBehaviour {

    private Vector3 direction;
    private Transform threatTransform;
    private Transform carTransform;  // To store the car's transform
    private bool isMovingTowardsCar = false;

    // Walking/running/changing direction
    [SerializeField] float walkSpeed = 1.5f;
    [SerializeField] float runSpeed = 4f;
    [SerializeField] float directionChangeInterval = 5f; // Time interval for changing direction
    [SerializeField] float detectionRange = 10f; // Range within which the hooker detects the zombie
    [SerializeField] float rotationSpeed = 25f; // Speed at which the hooker turns

    // Flee
    [SerializeField] float fieldOfViewAngle = 360f; // Angle in degrees for frontal vision
    [SerializeField] float fleeTimeout = 5f; // Time in seconds before stopping the flee when out of range
    private bool isFleeing;
    private float timeSinceLastDetection = 0f;

    // Ground check
    [SerializeField] LayerMask groundLayer; // Layer mask for the ground
    [SerializeField] float groundCheckDistance = 0.2f; // Distance to check for ground
    private bool isGrounded;

    public static event System.Action<HookerAI> OnHookerDeath;

    // Animation fields
    private bool isWalking;
    private bool isRunning;

    void Start() {
        // Start changing direction periodically
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update() {
        CheckGroundStatus();

        if (isGrounded) {
            if (isMovingTowardsCar && carTransform != null) {
                MoveTowardsCar(); // Prioritize moving towards the car
            } else if (threatTransform != null) {
                HandleThreatDetection(); // Handle fleeing from zombies
            } else {
                DetectThreat(); // Regular detection for a new threat
            }

            // Handle random walking if not moving towards the car or fleeing
            if (!isMovingTowardsCar && !isFleeing) {
                RandomWalk();
            }
        }
    }

    private void MoveTowardsCar() {
        // Calculate the direction towards the car
        Vector3 directionToCar = (carTransform.position - transform.position).normalized;

        // Rotate towards the car
        Quaternion targetRotation = Quaternion.LookRotation(directionToCar);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards the car
        transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);

        // Check if the hooker is close enough to the car to be destroyed
        if (Vector3.Distance(transform.position, carTransform.position) < 1.5f) {
            OnReachCar();
        }
    }

    private void HandleThreatDetection() {
        float distanceToThreat = Vector3.Distance(transform.position, threatTransform.position);

        if (distanceToThreat <= detectionRange) {
            isRunning = true;
            isFleeing = true;
            timeSinceLastDetection = 0f; // Reset timer when detecting the threat
            FleeFromThreat();
        } else {
            // Increment timer when threat is out of range
            timeSinceLastDetection += Time.deltaTime;

            if (timeSinceLastDetection >= fleeTimeout) {
                StopFleeing();
            }
        }
    }

    private void FleeFromThreat() {
        // Calculate the direction away from the threat
        Vector3 directionAwayFromThreat = (transform.position - threatTransform.position).normalized;

        // Rotate away from the threat
        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromThreat);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move away from the threat
        transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
    }

    private void StopFleeing() {
        threatTransform = null;
        isRunning = false;
        isFleeing = false;
    }

    private void RandomWalk() {
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);
        }

        // Update the walking state
        isWalking = direction != Vector3.zero && !isRunning;
    }

    IEnumerator ChangeDirectionRoutine() {
        while (true) {
            if (!isRunning && !isMovingTowardsCar) {
                yield return new WaitForSeconds(directionChangeInterval);
                direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                isWalking = true;
            } else {
                yield return null;
            }
        }
    }

    void DetectThreat() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        bool foundThreat = false;

        foreach (var hitCollider in hitColliders) {
            if (hitCollider.CompareTag("Zombie")) {
                Vector3 directionToZombie = (hitCollider.transform.position - transform.position).normalized;
                float angleToZombie = Vector3.Angle(transform.forward, directionToZombie);

                if (angleToZombie <= fieldOfViewAngle) {
                    threatTransform = hitCollider.transform;
                    isRunning = true;
                    isFleeing = true;
                    foundThreat = true;
                    timeSinceLastDetection = 0f;
                    break;
                }
            }
        }

        if (!foundThreat && isFleeing) {
            timeSinceLastDetection += Time.deltaTime;

            if (timeSinceLastDetection >= fleeTimeout) {
                StopFleeing();
            }
        }
    }

    void CheckGroundStatus() {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);

        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance, Color.green);
    }

    public void MoveTowardsCar(Transform carTransform) {
        this.carTransform = carTransform;
        isMovingTowardsCar = true;
        threatTransform = null;  // Stop fleeing from zombies
        isFleeing = false;
        isRunning = false;

        // Calculate the direction towards the car
        Vector3 directionToCar = (carTransform.position - transform.position).normalized;

        // Rotate towards the car
        Quaternion targetRotation = Quaternion.LookRotation(directionToCar);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards the car
        transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);

        // Check if the hooker is close enough to the car to be destroyed
        if (Vector3.Distance(transform.position, carTransform.position) < 4f) {  // Increased the proximity distance to 3f
            OnReachCar();
        }
    }


    private void OnReachCar() {
        GameManager.Instance.AddScore(1);
        Destroy(gameObject);
    }

    private void OnDestroy() {
        OnHookerDeath?.Invoke(this);
    }

    public bool IsWalking() {
        return isWalking;
    }

    public bool IsRunning() {
        return isRunning;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        Vector3 forward = transform.forward * detectionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position + leftBoundary, transform.position + rightBoundary);

        DrawFOVCone();
    }

    void DrawFOVCone() {
        Vector3 forward = transform.forward * detectionRange;
        int segments = 30;
        float angleStep = fieldOfViewAngle / segments;

        for (int i = 0; i <= segments; i++) {
            float angle = -fieldOfViewAngle / 2 + i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }
}
