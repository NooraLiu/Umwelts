using UnityEngine;

public class BirdOnlyVisibility : MonoBehaviour
{
    [Tooltip("Reference to the UmweltCameraController in the scene.")]
    public UmweltCameraController controller;

    private Renderer[] renderers;

    void Start()
    {
        if (controller == null)
        {
            Debug.LogError("BirdOnlyVisibility: Please assign the UmweltCameraController reference in the Inspector.");
            enabled = false;
            return;
        }

        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    void Update()
    {
        if (controller.CurrentMode == UmweltCameraController.Mode.Bird)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null && !r.enabled)
                    r.enabled = true;
            }
        }
        else
        {
            foreach (Renderer r in renderers)
            {
                if (r != null && r.enabled)
                    r.enabled = false;
            }
        }
    }
}
