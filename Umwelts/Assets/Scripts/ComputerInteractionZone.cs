using UnityEngine;

public class ComputerInteractionZone : MonoBehaviour
{
    public GameObject computerScreenImage; // Image to show/hide
    public float interactionRadius = 2f;
    public KeyCode interactionKey = KeyCode.Space;
    public GameObject nextObject;

    private bool playerInRange;
    private Transform player;
    private UmweltCameraController cameraController;
    private bool isImageActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            cameraController = player.GetComponent<UmweltCameraController>();
        }

        // Ensure the image is hidden initially
        if (computerScreenImage != null)
        {
            computerScreenImage.SetActive(false);
        }

        // Add sphere collider for visual debugging
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = interactionRadius;
        sc.isTrigger = true;
    }

    void Update()
    {

        if (playerInRange && cameraController != null && cameraController.CurrentMode == UmweltCameraController.Mode.Person && Input.GetKeyDown(interactionKey))
        {
            ToggleComputerScreen();
            nextObject.gameObject.SetActive(true);
            Debug.Log("Computer!"); // Check if this logs in Unity
        }
    }

    void ToggleComputerScreen()
    {
        if (computerScreenImage != null)
        {
            isImageActive = !isImageActive;
            computerScreenImage.SetActive(isImageActive);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (computerScreenImage != null)
        {
            computerScreenImage.SetActive(false);
        }
        }
    }
}
