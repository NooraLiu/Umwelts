using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bed : MonoBehaviour
{
    public float interactionRadius = 2f;
    public KeyCode interactionKey = KeyCode.Space;
    public TextMeshProUGUI Text;
    public static bool meditation = false; // Ensuring meditation state is accessible

    private bool playerInRange;
    private Transform player;
    private UmweltCameraController cameraController;
    private int interactionStage = 0; // Tracks the interaction steps

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            cameraController = player.GetComponent<UmweltCameraController>();
        }

        // Ensure the text is hidden initially
        if (Text != null)
        {
            Text.gameObject.SetActive(false);
        }

        // Add sphere collider for interaction detection
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = interactionRadius;
        sc.isTrigger = true;
    }

    void Update()
    {
        if (playerInRange && cameraController != null && cameraController.CurrentMode == UmweltCameraController.Mode.Person && Input.GetKeyDown(interactionKey))
        {
            HandleInteraction();
        }
    }

   void HandleInteraction()
{
    if (interactionStage == 0)
    {
        // First press: Show text
        if (Text != null)
        {
            Text.gameObject.SetActive(true);
        }
    }
    else if (interactionStage == 1)
    {
        // Start meditation
        MeditationSoundscape soundscape = FindObjectOfType<MeditationSoundscape>();
        if (soundscape != null)
        {
            soundscape.meditation = true;
            Debug.Log("Meditation started in MeditationSoundscape!");
        }

        // Disable Character Controller to prevent movement
        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false; // Prevent movement during meditation
            }
        }

        // Hide interaction text when meditation begins
        if (Text != null)
        {
            Text.gameObject.SetActive(false);
        }
    }

    interactionStage++; // Move to the next interaction stage
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

            // Reset the interaction state when leaving the area
            interactionStage = 0;

            if (Text != null)
            {
                Text.gameObject.SetActive(false);
            }
        }
    }
}
