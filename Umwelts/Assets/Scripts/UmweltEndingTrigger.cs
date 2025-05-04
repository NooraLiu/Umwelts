using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UmweltEndingTrigger : MonoBehaviour
{
    public float autoMoveSpeed = 2f;
    public float fadeDuration = 2f;

    public TMP_Text titleText;      // TextMeshPro Text
    public Image fadeOverlay;       // Fullscreen UI Image (white, alpha 0 initially)

    private bool triggered = false;
    private CharacterController controller;
    private UmweltCameraController camController;
    private float fadeTimer = 0f;
    private Vector3 moveDirection;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        camController = other.GetComponent<UmweltCameraController>();
        if (camController != null)
        {
            controller = camController.GetComponent<CharacterController>();
            triggered = true;

            camController.ending = true;
            camController.enabled = false;

            moveDirection = camController.transform.forward;

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.color = new Color(1f, 1f, 1f, 0f); // white, fully transparent
            }

            if (titleText != null)
                titleText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!triggered || controller == null) return;

        // Keep hovering and auto-move forward horizontally
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized * autoMoveSpeed;
        controller.Move(horizontalMove * Time.deltaTime);

        // Fade screen to white
        fadeTimer += Time.deltaTime;
        float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
        if (fadeOverlay != null)
            fadeOverlay.color = new Color(1f, 1f, 1f, alpha);

        // Show Umwelt text after fade is done
        if (alpha >= 1f && titleText != null && !titleText.gameObject.activeSelf)
        {
            titleText.gameObject.SetActive(true);
        }
    }
}
