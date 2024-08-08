using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleMovement : MonoBehaviour {
    private PlayerControls playerControls;
    private Rigidbody rb;

    public float accelerationForce = 150f;
    public float turnSpeed = 100f;
    public float maxSteerAngle = 30f;  // Maximum steering angle in degrees

    public WheelCollider[] wheelColliders;
    public Transform[] wheelMeshes;

    // Reverse for camera
    private Vector2 inputVector;

    private void Awake() {
        playerControls = new PlayerControls();
        playerControls.Vehicle.Enable();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable() {
        playerControls.Vehicle.Enable();
    }

    private void OnDisable() {
        playerControls.Vehicle.Disable();
    }

    private void FixedUpdate() {
        // Update the existing inputVector field
        inputVector = GetMovementVectorNormalized();

        // Calculate forward force
        float motorTorque = inputVector.y * accelerationForce;
        foreach (WheelCollider wheel in wheelColliders) {
            wheel.motorTorque = motorTorque;
        }

        // Calculate turning force or rotation, with a max steering angle
        float steer = Mathf.Clamp(inputVector.x * turnSpeed, -maxSteerAngle, maxSteerAngle);
        for (int i = 0; i < wheelColliders.Length; i++) {
            if (i < 2) { // Assuming the first two are front wheels
                wheelColliders[i].steerAngle = steer;
            }
        }

        UpdateWheelMeshes();
    }

    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerControls.Vehicle.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    //Camera Reverse bool
    public bool IsReversing() {
        return inputVector.y < 0;
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
}
