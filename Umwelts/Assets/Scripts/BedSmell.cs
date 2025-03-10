using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedSmell : MonoBehaviour
{
    public GameObject uiText; // Assign the UI Text GameObject in the Inspector
    public float interactionRadius = 2f; // Set the detection radius

    private Transform player;
    private UmweltCameraController cameraController;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            cameraController = player.GetComponent<UmweltCameraController>();
        }

        // Ensure the UI text is hidden initially
        if (uiText != null)
        {
            uiText.SetActive(false);
        }

        // Add a trigger collider to the Particle System object if not present
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.radius = interactionRadius;
        sc.isTrigger = true;
    }

    void Update()
    {
        // Show UI text only if player is in range and in Dog Mode
        if (playerInRange && cameraController != null && cameraController.CurrentMode == UmweltCameraController.Mode.Dog)
        {
            uiText?.SetActive(true);
        }
        else
        {
            uiText?.SetActive(false);
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
            uiText?.SetActive(false); // Hide UI text when player leaves
        }
    }
}
