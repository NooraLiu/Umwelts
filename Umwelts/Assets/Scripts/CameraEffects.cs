using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public UmweltCameraController cameraController; // Reference to the player controller
    public Transform playerTransform; // Reference to the player position

    [Header("Bobbing Settings")]
    public float walkBobbingSpeed = 3f;
    public float walkBobbingAmount = 0.05f;
    public float runBobbingSpeed = 5f;
    public float runBobbingAmount = 0.1f;

    public float dogBobbingSpeed = 4f;
    public float dogBobbingAmount = 0.08f;

    public float birdBobbingSpeed = 2f;
    public float birdBobbingAmount = 0.02f;

    private float timer = 0;
    private Vector3 initialLocalPosition;
    private Vector3 targetPosition;

    void Start()
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<UmweltCameraController>();
        }

        if (playerTransform == null)
        {
            playerTransform = cameraController.transform; // Get the player's transform
        }

        initialLocalPosition = transform.localPosition; // Store camera holder’s original position
    }

    void ApplyBobbing()
    {
        if (!cameraController.IsGrounded && cameraController.CurrentMode != UmweltCameraController.Mode.Bird) return;

        float speed = 0;
        float amount = 0;

        switch (cameraController.CurrentMode)
        {
            case UmweltCameraController.Mode.Person:
                speed = cameraController.IsSprinting ? runBobbingSpeed : walkBobbingSpeed;
                amount = cameraController.IsSprinting ? runBobbingAmount : walkBobbingAmount;
                break;

            case UmweltCameraController.Mode.Dog:
                speed = dogBobbingSpeed;
                amount = dogBobbingAmount;
                break;

            case UmweltCameraController.Mode.Bird:
                if (cameraController.IsGrounded) return; // No bobbing on ground for birds
                speed = birdBobbingSpeed;
                amount = birdBobbingAmount;
                break;
        }

        if (cameraController.Velocity.magnitude > 0.1f)
        {
            timer += Time.deltaTime * speed;
            float bobbingOffset = Mathf.Sin(timer) * amount;

            // Apply only the Y-axis bobbing to the CameraHolder
            transform.position += new Vector3(0, bobbingOffset, 0);
        }
        else
        {
            timer = 0;
        }
    }
}