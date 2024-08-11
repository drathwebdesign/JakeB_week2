using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class VehicleMovement2 : MonoBehaviour {
    private PlayerControls playerControls;
    private Rigidbody rb;

    [SerializeField] float accelerationForce = 1500f;
    [SerializeField] float turnSpeed = 100f;
    [SerializeField] float maxSteerAngle = 30f;
    [SerializeField] float downforce = 100f;
    [SerializeField] float handbrakeForce = 3000f;
    [SerializeField] float nitrosMultiplier = 2f;  // Multiplier for the nitros boost
    public float maxNitros = 100f;  // Max nitros capacity
    [SerializeField] float nitrosConsumptionRate = 5f;  // How fast nitros depletes when used

    public Slider nitrosSlider;
    private float currentNitros;  // Current nitros amount
    private bool isNitrosActive = false;
    private bool isHandbrakeActive = false;
    private Vector2 inputVector;
    private Transform[] respawnPoints;

    public AudioClip hornClip;  // The horn sound clip
    private AudioSource audioSource;

    public WheelCollider[] wheelColliders;
    public Transform[] wheelMeshes;


    private void Awake() {
        playerControls = new PlayerControls();
        playerControls.Vehicle2.Enable();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Assign input actions
        playerControls.Vehicle2.Horn.performed += ctx => HonkHorn();
        playerControls.Vehicle2.Handbrake.started += ctx => isHandbrakeActive = true;
        playerControls.Vehicle2.Handbrake.canceled += ctx => isHandbrakeActive = false;
        playerControls.Vehicle2.Nitros.started += ctx => StartNitros();
        playerControls.Vehicle2.Nitros.canceled += ctx => StopNitros();
        playerControls.Vehicle2.Unstuck.started += ctx => UnstuckVehicle();

        // Initialize nitros amount
        currentNitros = maxNitros;

        // Find all respawn points with the tag "RespawnPoint"
        FindRespawnPoints();
    }

    private void Start() {
        // Subscribe to the zombie death event
        ZombieAI.OnAnyZombieDeath += HandleZombieDeath;
    }

    private void OnEnable() {
        playerControls.Vehicle2.Enable();
    }

    private void OnDisable() {
        playerControls.Vehicle2.Disable();
    }

    private void OnDestroy() {
        // Unsubscribe from the zombie death event to prevent memory leaks
        ZombieAI.OnAnyZombieDeath -= HandleZombieDeath;
    }

    private void Update() {
        // Example of nitros depletion logic if nitros is active
        if (isNitrosActive && currentNitros > 0) {
            currentNitros -= Time.deltaTime * 10f; // Adjust depletion rate as needed

            // Update the slider
            if (nitrosSlider != null) {
                nitrosSlider.value = currentNitros;
            }

            // Stop nitros if it runs out
            if (currentNitros <= 0) {
                StopNitros();
            }
        }
    }

    private void FixedUpdate() {
        // Update the existing inputVector field
        inputVector = GetMovementVectorNormalized();

        // Calculate forward force with optional nitros boost
        float motorTorque = inputVector.y * accelerationForce;
        if (isNitrosActive && currentNitros > 0) {
            motorTorque *= nitrosMultiplier;
            currentNitros -= nitrosConsumptionRate * Time.deltaTime;
            if (currentNitros <= 0) {
                StopNitros();  // Stop nitros if it runs out
            }
        }

        // Apply motor torque to the wheels
        foreach (WheelCollider wheel in wheelColliders) {
            if (isHandbrakeActive) {
                // Lock rear wheels if handbrake is active
                if (wheel == wheelColliders[2] || wheelColliders[3]) {
                    wheel.brakeTorque = handbrakeForce;
                    wheel.motorTorque = 0;
                } else {
                    wheel.brakeTorque = 0;
                    wheel.motorTorque = motorTorque;
                }
            } else {
                wheel.brakeTorque = 0;
                wheel.motorTorque = motorTorque;
            }
        }

        // Calculate turning force with a max steering angle
        float steer = Mathf.Clamp(inputVector.x * turnSpeed, -maxSteerAngle, maxSteerAngle);
        for (int i = 0; i < wheelColliders.Length; i++) {
            if (i < 2) { // Assuming the first two are front wheels
                wheelColliders[i].steerAngle = steer;
            }
        }

        // Apply downforce to stabilize the vehicle at higher speeds
        rb.AddForce(-transform.up * downforce * rb.velocity.magnitude);

        UpdateWheelMeshes();
    }

    private Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerControls.Vehicle2.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    // Honk the horn
    private void HonkHorn() {
        audioSource.PlayOneShot(hornClip);
        Debug.Log("Honk Horn!");

        Collider[] hookersInRange = Physics.OverlapSphere(transform.position, 12f);
        foreach (Collider collider in hookersInRange) {
            if (collider.CompareTag("Hooker")) {
                HookerAI hookerAI = collider.GetComponent<HookerAI>();
                if (hookerAI != null) {
                    hookerAI.MoveTowardsCar(transform);
                }
            }
        }
    }


    // Unstuck vehicle by moving it to the closest respawn point
    private void UnstuckVehicle() {
        if (respawnPoints == null || respawnPoints.Length == 0) {
            Debug.LogWarning("No respawn points found in the scene.");
            return;
        }

        Transform closestRespawn = null;
        float closestDistance = Mathf.Infinity;

        foreach (var respawnPoint in respawnPoints) {
            float distance = Vector3.Distance(transform.position, respawnPoint.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestRespawn = respawnPoint;
            }
        }

        if (closestRespawn != null) {
            transform.position = closestRespawn.position;
            transform.rotation = closestRespawn.rotation;
            rb.velocity = Vector3.zero;  // Reset velocity
            rb.angularVelocity = Vector3.zero;  // Reset angular velocity
        }
    }

    private void UpdateWheelMeshes() {
        for (int i = 0; i < wheelColliders.Length; i++) {
            WheelHit hit;
            wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheelMeshes[i].position = position;
            wheelMeshes[i].rotation = rotation;

            // Optional: Use hit information for debugging or visual adjustments
            if (wheelColliders[i].GetGroundHit(out hit)) {
                // Debug.Log($"Wheel {i} hit point: {hit.point}, normal: {hit.normal}");
                // Example: Use hit data for adjusting visual elements or effects
            }
        }
    }

    // Finds all respawn points in the scene with the "RespawnPoint" tag
    private void FindRespawnPoints() {
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("RespawnPoint");
        respawnPoints = new Transform[respawnObjects.Length];
        for (int i = 0; i < respawnObjects.Length; i++) {
            respawnPoints[i] = respawnObjects[i].transform;
        }
    }

    // Start nitros boost
    private void StartNitros() {
        if (currentNitros > 0) {
            isNitrosActive = true;
            Debug.Log("Nitros Activated!");
        }
    }

    // Stop nitros boost
    private void StopNitros() {
        isNitrosActive = false;
        Debug.Log("Nitros Deactivated!");
    }

    // Refill nitros (call this method when an item is picked up)
    public void RefillNitros(float amount) {
        currentNitros = Mathf.Clamp(currentNitros + amount, 0, maxNitros);
        Debug.Log("Nitros Refilled! Current nitros: " + currentNitros);
        // Update the slider
        if (nitrosSlider != null) {
            nitrosSlider.value = currentNitros;
        }
    }

    private void HandleZombieDeath() {
        // Gain 10 nitros when a zombie is killed
        RefillNitros(10f);
    }

    // Camera Reverse bool
    public bool IsReversing() {
        return inputVector.y < 0;
    }
}