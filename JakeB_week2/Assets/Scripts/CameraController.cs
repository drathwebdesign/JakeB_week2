using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour {
    public CinemachineVirtualCamera normalCamera;
    public CinemachineVirtualCamera reverseCamera;

    private PlayerControls playerControls;
    private bool isReverseCameraActive = false;

    private void Awake() {
        playerControls = new PlayerControls();
        playerControls.Vehicle.Enable();
        playerControls.Vehicle.ReverseCamera.performed += _ => SwitchCamera();
    }

    private void OnEnable() {
        playerControls.Vehicle.Enable();
    }

    private void OnDisable() {
        playerControls.Vehicle.Disable();
    }

    private void SwitchCamera() {
        isReverseCameraActive = !isReverseCameraActive;

        if (isReverseCameraActive) {
            reverseCamera.Priority = 10;
            normalCamera.Priority = 1;
        } else {
            normalCamera.Priority = 10;
            reverseCamera.Priority = 1;
        }
    }
}