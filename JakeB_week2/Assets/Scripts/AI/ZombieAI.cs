using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour {

    private Vector3 direction;
    private Transform targetTransform;
    private Rigidbody rb;

    // Walking/running/changing direction
    [SerializeField] float walkSpeed = 1.5f;
    [SerializeField] float runSpeed = 4f;
    [SerializeField] float directionChangeInterval = 5f; // Time interval for changing direction
    [SerializeField] float detectionRange = 15f; // Range within which the zombie detects the target
    [SerializeField] float rotationSpeed = 25f; // Speed at which the zombie turns

    // Aggro
    [SerializeField] float fieldOfViewAngle = 60f; // Angle in degrees for frontal vision
    [SerializeField] float aggroTimeout = 5f; // Time in seconds before losing aggro when out of range
    private bool isAggroed;
    private float timeSinceLastDetection = 0f;

    // Attack
    [SerializeField] float attackMovementSpeed = 0.5f;
    [SerializeField] float attackRange = 2.0f;

    // Ground check
    [SerializeField] LayerMask groundLayer; // Layer mask for the ground
    [SerializeField] float groundCheckDistance = 0.2f; // Distance to check for ground
    private bool isGrounded;

    [SerializeField] float carHitVelocityThreshold = 20f; // Velocity threshold for death
    public delegate void ZombieDeathHandler(ZombieAI zombie);
    public static event ZombieDeathHandler OnZombieDeath;
    public delegate void ZombieDeathAction();
    public static event System.Action OnAnyZombieDeath;

    // Animation fields
    private bool isWalking;
    private bool isRunning;
    private bool isAttacking;
    private bool isDieing = false;
  

    void Start() {
        rb = GetComponent<Rigidbody>();
        // Start changing direction periodically
        StartCoroutine(ChangeDirectionRoutine());
        LockRotationConstraints();
    }

    void Update() {
        CheckGroundStatus();
        if (isGrounded) {
            if (targetTransform != null) {
                // Track target while it's within the aggro range
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (distanceToTarget <= attackRange) {
                    // Attack range reached
                    isAttacking = true;
                    isRunning = false;
                    isWalking = false;
                    timeSinceLastDetection = 0f; // Reset timer when attacking
                } else if (distanceToTarget <= detectionRange) {
                    DetectTarget();
                    isAttacking = false;
                    timeSinceLastDetection = 0f; // Reset timer when detecting the target
                } else {
                    // Increment timer when target is out of range
                    timeSinceLastDetection += Time.deltaTime;

                    if (timeSinceLastDetection >= aggroTimeout) {
                        // Stop pursuing if the timer exceeds the aggro timeout
                        targetTransform = null;
                        isRunning = false;
                        isAggroed = false;
                        isAttacking = false;
                    }
                }
            } else {
                DetectTarget(); // Regular detection for a new target
                isAttacking = false;
            }

            if (isAttacking && targetTransform != null) {
                // Handle reduced movement while attacking
                transform.Translate(Vector3.forward * attackMovementSpeed * Time.deltaTime);
            } else {
                if (isRunning && targetTransform != null) {
                    // Calculate the direction towards the target
                    Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

                    // Rotate towards the target
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    // Move towards the target
                    transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
                } else if (isWalking) {
                    // Move in the current direction and rotate towards it
                    if (direction != Vector3.zero) {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                        transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);
                    }
                }

                // Update the walking state
                isWalking = direction != Vector3.zero && !isRunning;
            }
        }
    }


    private void LateUpdate() {

    }

    IEnumerator ChangeDirectionRoutine() {
        while (!isDieing) // Only change direction if not dying
        {
            if (!isRunning && !isAttacking) {
                yield return new WaitForSeconds(directionChangeInterval);
                direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            } else {
                yield return null; // Continue checking for the target
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Check if the collided object is a lamppost
        if (collision.gameObject.CompareTag("Knockable")) {
            // Prevent the zombie from affecting the lamppost
            Rigidbody knockableRb = collision.rigidbody;
            if (knockableRb != null) {
                knockableRb.isKinematic = true; // Make the lamppost kinematic
                StartCoroutine(ResetKinematic(knockableRb)); // Reset after a short delay
            }
        }
        //XYZ
        if (collision.gameObject.CompareTag("Player")) {
            UnlockRotationConstraints();
            StartCoroutine(ReapplyRotationConstraintsAfterDelay(5f));
        }

        // Check if the collided object is a car
        if (collision.gameObject.CompareTag("Player")) {
            float impactVelocity = collision.relativeVelocity.magnitude;

            if (impactVelocity > carHitVelocityThreshold) {
                Die(); // Call Die method if the impact velocity exceeds the threshold
            }
        }
    }
    private void LockRotationConstraints() {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void UnlockRotationConstraints() {
        rb.constraints = RigidbodyConstraints.None;
    }

    private System.Collections.IEnumerator ResetKinematic(Rigidbody rb) {
        yield return new WaitForSeconds(0.1f); // Short delay before resetting
        rb.isKinematic = false; // Restore the lamppost's non-kinematic state
    }
    private IEnumerator ReapplyRotationConstraintsAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        LockRotationConstraints();
    }

    void DetectTarget() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        bool foundTarget = false;

        foreach (var hitCollider in hitColliders) {
            if (hitCollider.CompareTag("Hooker")) {
                Vector3 directionToHooker = (hitCollider.transform.position - transform.position).normalized;
                float angleToHooker = Vector3.Angle(transform.forward, directionToHooker);

                if (angleToHooker <= fieldOfViewAngle) {
                    targetTransform = hitCollider.transform;
                    isRunning = true;
                    isAggroed = true;
                    foundTarget = true;
                    timeSinceLastDetection = 0f; // Reset timer on detection
                    break;
                }
            }
        }

        if (!foundTarget) {
            // If no target was found, but zombie was previously aggroed, keep chasing
            if (isAggroed) {
                // Increment the timer to check if we should lose aggro
                timeSinceLastDetection += Time.deltaTime;

                if (timeSinceLastDetection >= aggroTimeout) {
                    // If the target is out of aggro range and timeout has been reached, stop pursuing
                    targetTransform = null;
                    isRunning = false;
                    isAggroed = false;
                }
            }
        }
    }

    // Check if the AI is grounded
    void CheckGroundStatus() {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);

        // Optional: Draw a debug line to visualize the ground check
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance, Color.green);
    }

    void Die() {
        if (isDieing)
            return;

        isDieing = true;

        // Notify listeners that this zombie has died
        OnZombieDeath?.Invoke(this);

        // Notify listeners that any zombie has died
        OnAnyZombieDeath?.Invoke();

        // Destroy this zombie after a delay
        Destroy(gameObject, 3f);
    }

    // Animations
    public bool IsWalking() {
        return isWalking;
    }

    public bool IsRunning() {
        return isRunning;
    }

    public bool IsAttacking() {
        return isAttacking;
    }
    public bool IsDieing() {
        return isDieing;
    }

    /// <summary>
    /// GIZMO Not affecting any scripting
    /// </summary>
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;

        // Draw the view frustum
        Vector3 forward = transform.forward * detectionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position + leftBoundary, transform.position + rightBoundary);

        // Optional: Draw the FOV cone as lines
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