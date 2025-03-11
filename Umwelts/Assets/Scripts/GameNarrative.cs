using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameNarrative : MonoBehaviour
{
    public TextMeshProUGUI narrativeText; // TextMeshPro text for displaying messages

    public TextMeshProUGUI wakeUpPrompt; // Instead of a button, show a text prompt

    public GameObject playerController; // The Player GameObject with CharacterController
    public Camera introCamera; // The camera for the intro scene
    public Image fadeOverlay; // UI Image for the fade-in effect
    public float fadeDuration = 3f; // Duration of fade-in
    public Canvas narrativeCanvas;
    public GameObject interactionZone;

    private float mouseSensitivity = 0.3f; // Controls how much the camera responds to mouse movement
    private float maxTilt = 90f; // Limits how much the camera can move from its initial position
    private Quaternion initialRotation; // Stores the camera's starting rotation
    private Quaternion targetRotation; // The target rotation for smooth movement

    void Start()
    {
        narrativeCanvas.gameObject.SetActive(true);
        interactionZone.gameObject.SetActive(false);

        // Ensure player starts inactive (so they don't fall)
        playerController.SetActive(false);

        // Ensure fadeOverlay starts fully black
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.color = new Color(0, 0, 0, 1); // Fully black

        wakeUpPrompt.gameObject.SetActive(false);

        // Store initial camera rotation (looking upwards)
        initialRotation = introCamera.transform.rotation;
        targetRotation = initialRotation; // Start with no rotation change

        // Start the intro sequence
        StartCoroutine(NarrativeSequence());
    }

    void Update()
    {
        if (introCamera.gameObject.activeSelf)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Calculate small rotation offsets
            Quaternion xRotation = Quaternion.AngleAxis(-mouseY * maxTilt, introCamera.transform.right);
            Quaternion yRotation = Quaternion.AngleAxis(mouseX * maxTilt, Vector3.up);

            // Compute target rotation (clamped movement)
            targetRotation = initialRotation * xRotation * yRotation;

            // Smoothly interpolate towards the target rotation
            introCamera.transform.rotation = Quaternion.Lerp(introCamera.transform.rotation, targetRotation, Time.deltaTime * 2f);
        }

        if (wakeUpPrompt.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            TurnOnLight();
        }
    }

    IEnumerator NarrativeSequence()
    {
        // Start long fade-in while displaying text
        StartCoroutine(FadeIn());

        // Display text sequence
        yield return StartCoroutine(ShowText("Tomorrow, 9 A.M.", 3));
        // yield return StartCoroutine(ShowText("A very important meeting...", 3));
        // yield return StartCoroutine(ShowText("Have I put it to my calendar?", 3));
        // yield return StartCoroutine(ShowText("One sheep.", 3));
        // yield return StartCoroutine(ShowText("What does grass taste like?", 3));
        // yield return StartCoroutine(ShowText("Two sheeps.", 3));
        // yield return StartCoroutine(ShowText("Cows have four stomachs.", 3));
        // yield return StartCoroutine(ShowText("Does each stomach taste grass differently?", 3));
        // yield return StartCoroutine(ShowText("Three sheeps.", 3));
        // yield return StartCoroutine(ShowText("Ah, that's stupid.", 3));
        // yield return StartCoroutine(ShowText("Stomachs don't taste... ", 3));
        // yield return StartCoroutine(ShowText("Or... do they?", 3));
        // yield return StartCoroutine(ShowText("Four sheeps.", 3));
        // yield return StartCoroutine(ShowText("......", 6));
        // yield return StartCoroutine(ShowText("WHY CAN'T I FALL ASLEEP?", 3));

        wakeUpPrompt.gameObject.SetActive(true);
    }

    void TurnOnLight()
    {
        // Turn off intro camera
        introCamera.gameObject.SetActive(false);

        // Enable player controller
        playerController.SetActive(true);

        wakeUpPrompt.gameObject.SetActive(false);

        narrativeCanvas.gameObject.SetActive(false);
        interactionZone.SetActive(true);

    }

    IEnumerator ShowText(string text, float duration)
    {
        narrativeText.text = text;
        narrativeText.alpha = 0.0f;

        // Fade in
        while (narrativeText.alpha < 1.0f)
        {
            narrativeText.alpha += Time.deltaTime / 0.5f;
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Fade out
        while (narrativeText.alpha > 0.0f)
        {
            narrativeText.alpha -= Time.deltaTime / 0.5f;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, 0); // Fully transparent
        fadeOverlay.gameObject.SetActive(false);
    }
}
