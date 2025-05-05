using UnityEngine;

public class PersonOnlyVisibility : MonoBehaviour
{
    private UmweltCameraController controller;

    void Start()
    {
        controller = FindObjectOfType<UmweltCameraController>();
        if (controller == null)
        {
            Debug.LogError("PersonOnlyVisibility: Could not find UmweltCameraController in the scene.");
            enabled = false;
        }
    }

    void Update()
    {
        bool isPerson = controller.CurrentMode == UmweltCameraController.Mode.Person;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = isPerson;
        }
    }
}
