using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Lamp : MonoBehaviour
{

    public float interactionRadius = 2f;
    public KeyCode interactionKey = KeyCode.Space;
    public GameObject nextObject;
    public TextMeshProUGUI Text;

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
        if (Text != null)
        {
            Text.gameObject.SetActive(false);
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

        // Ensure the next object is only activated if it's inactive
        if (nextObject != null && !nextObject.activeSelf)
        {
            nextObject.gameObject.SetActive(true);
            Debug.Log("Activated: " + nextObject.name);
        }
    }
    }

    void ToggleComputerScreen()
    {
        if (Text != null)
        {
            isImageActive = !isImageActive;
            Text.gameObject.SetActive(isImageActive);
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
        }
    }
}
