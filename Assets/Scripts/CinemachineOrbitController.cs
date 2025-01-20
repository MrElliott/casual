using System;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineOrbitController : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("Target Settings")]
    public Transform target;

    [Header("Control Settings")]
    public float rotationSpeed = 100f;
    public float panSpeed = 5f;
    public float zoomSpeed = 5f;
    public float pitchSpeed = 50f;
    public float minPitch = -30f;
    public float maxPitch = 80f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private Vector3 followOffset;
    private float currentYaw;
    private float currentPitch;
    private float currentZoom;

    private Vector3 panOffset; // Stores the panning offset

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("Target is not assigned!");
            return;
        }

        UpdateVariables();
    }

    private void Update()
    {
        if (virtualCamera == null || target == null)
            return;

        // Handle rotation around the target
        if (Input.GetMouseButton(1)) // Right Mouse Button
        {
            float yawInput = Input.GetAxis("Mouse X");
            float pitchInput = -Input.GetAxis("Mouse Y"); // Negative to invert pitch
            currentYaw += yawInput * rotationSpeed * Time.deltaTime;
            currentPitch += pitchInput * pitchSpeed * Time.deltaTime;

            // Clamp pitch to avoid flipping
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        // Handle panning
        if (Input.GetMouseButton(2)) // Middle Mouse Button
        {
            float panX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float panY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            // Adjust the pan offset
            panOffset += new Vector3(panX, panY, 0);
        }

        // Handle zooming
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= zoomInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Calculate the camera's offset relative to the target
        Vector3 offset = new Vector3(0, 0, -currentZoom);
        offset = Quaternion.Euler(currentPitch, currentYaw, 0) * offset;

        // Apply the pan offset
        Vector3 finalPosition = target.position + panOffset + offset;

        // Update the camera's position and look at the target
        virtualCamera.transform.position = finalPosition;
        virtualCamera.transform.LookAt(target.position + panOffset);
    }

    public void SetTarget(Transform newTarget){
        target = newTarget;
        
        UpdateVariables();
    }

    private void UpdateVariables(){
        // Initialize zoom and offset
        CinemachineCameraOffset cameraOffset = virtualCamera.GetComponent<CinemachineCameraOffset>();
        if (cameraOffset == null)
            cameraOffset = virtualCamera.gameObject.AddComponent<CinemachineCameraOffset>();

        followOffset = cameraOffset.Offset;
        currentZoom = followOffset.magnitude;

        // Initialize pitch and yaw based on the offset direction
        currentPitch = Mathf.Asin(followOffset.normalized.y) * Mathf.Rad2Deg;
        currentYaw = target.eulerAngles.y;

        panOffset = Vector3.zero; // Initialize the pan offset
    }
}
