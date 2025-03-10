using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target; // The target the camera will orbit around
    public Vector3 offset = new Vector3(0, 2, -5); // Editable offset in the inspector
    public float sensitivity = 10f; // Sensitivity of the orbit
    public float minPitch = -20f; // Minimum pitch value
    public float maxPitch = 80f; // Maximum pitch value

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public float initialYaw = 0f;
    public float initialPitch = 0f;

    private Quaternion lastRotation;
    private float lastRotationTime;

    // Auto-spin
    public float inactivityTime = 5f; // Time in seconds to start auto-spinning after inactivity
    public float autoSpinSpeed = 10f; // Speed of auto-spin
    private float lastUserInteractionTime;

    // Zooming
    public float zoomSpeed = 2f; // Speed of zooming
    public float minZoom = 2f; // Minimum zoom distance
    public float maxZoom = 10f; // Maximum zoom distance
    private float currentZoom; // Current zoom level

    void Start()
    {
        yaw = initialYaw;
        pitch = Mathf.Clamp(initialPitch, minPitch, maxPitch);
        currentZoom = offset.magnitude; // Set zoom based on initial offset

        // Set initial position and rotation
        Quaternion initialRotation = Quaternion.Euler(pitch, yaw, 0.0f);
        transform.position = target.position + initialRotation * offset;
        transform.LookAt(target.position);

        lastRotation = initialRotation;
        lastRotationTime = Time.time;
        lastUserInteractionTime = Time.time;
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            lastUserInteractionTime = Time.time;

            float mouseX = sensitivity * Input.GetAxis("Mouse X");
            float mouseY = sensitivity * Input.GetAxis("Mouse Y");

            float intendedYaw = yaw + mouseX;
            float intendedPitch = pitch - mouseY;

            // Clamp the intended pitch within the specified range
            intendedPitch = Mathf.Clamp(intendedPitch, minPitch, maxPitch);

            // Calculate the intended rotation
            Quaternion intendedRotation = Quaternion.Euler(intendedPitch, intendedYaw, 0.0f);

            // Check the angle difference and the time since the last rotation
            if (Quaternion.Angle(lastRotation, intendedRotation) <= 20 || Time.time - lastRotationTime >= 0.1f)
            {
                ApplyRotation(intendedYaw, intendedPitch, intendedRotation);
            }
        }
        else if (Time.time - lastUserInteractionTime > inactivityTime)
        {
            // Auto-spin the camera if there has been no user interaction for the specified time
            yaw += autoSpinSpeed * Time.deltaTime;
            Quaternion autoSpinRotation = Quaternion.Euler(pitch, yaw, 0.0f);
            ApplyRotation(yaw, pitch, autoSpinRotation);
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            currentZoom -= scrollInput * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

        // Apply zoom by adjusting the offset
        Vector3 zoomedOffset = offset.normalized * currentZoom;
        transform.position = target.position + Quaternion.Euler(pitch, yaw, 0.0f) * zoomedOffset;
        transform.LookAt(target.position);
    }

    void ApplyRotation(float newYaw, float newPitch, Quaternion rotation)
    {
        yaw = newYaw;
        pitch = newPitch;
        transform.position = target.position + rotation * offset.normalized * currentZoom;
        transform.LookAt(target.position);

        lastRotation = rotation;
        lastRotationTime = Time.time;
    }
}
