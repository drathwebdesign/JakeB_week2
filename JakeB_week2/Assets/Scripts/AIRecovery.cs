using UnityEngine;

public class CharacterRecovery : MonoBehaviour {
    public float recoveryForce = 5f; // Gentle upward force to help the character stand up
    public float zRecoveryTorque = 50f; // Torque to correct Z-axis rotation
    public float recoveryAngle = 45f; // Angle threshold to consider the character as fallen
    public float maxFallingSpeed = 2f; // Max speed threshold to apply force

    private Rigidbody rb;
    private bool isFallen = false;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        if (isFallen) {
            RecoverFromFall();
        }
    }

    void RecoverFromFall() {
        // Check if the character is fallen by measuring the angle between the character's up vector and the world up vector
        float angle = Vector3.Angle(Vector3.up, transform.up);

        if (angle > recoveryAngle) {
            // Apply upward force only if the falling speed is below a threshold
            if (rb.velocity.y < maxFallingSpeed) {
                rb.AddForce(Vector3.up * recoveryForce, ForceMode.VelocityChange);
            }

            // Apply torque to correct the Z-axis rotation
            Vector3 torque = new Vector3(0, 0, -transform.rotation.eulerAngles.z) * zRecoveryTorque;
            rb.AddTorque(torque, ForceMode.VelocityChange);

            // Optionally, reset isFallen after applying recovery forces
            isFallen = false;
        }
    }

    // Call this method when the character is detected as fallen
    public void SetFallen(bool fallen) {
        isFallen = fallen;
    }
}