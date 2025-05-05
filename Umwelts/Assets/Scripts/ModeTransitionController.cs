using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeTransitionController : MonoBehaviour
{
    public Transform player;
    public ParticleSystem interactionZone; // Use its transform.position as center
    public float interactionRadius = 3f;

    public TextMeshProUGUI interactionHintText;
    public TextMeshProUGUI narrativeTextDisplay;
    public TextMeshProUGUI birdHintText;
    public Image fadeScreen;

    public List<string> narrativeTexts;
    public float textFadeTime = 0.5f;
    public float textHoldTime = 2f;
    public float fadeToBlackDuration = 2f;
    public float fadeFromBlackDuration = 1.5f;
    public float birdHintDuration = 3f;

    private bool inZone = false;
    private bool transitioning = false;

    private UmweltCameraController cameraController;


    void Start()
    {
         cameraController = FindObjectOfType<UmweltCameraController>();

        if (interactionHintText != null)
            interactionHintText.gameObject.SetActive(false);

        if (narrativeTextDisplay != null)
        {
            narrativeTextDisplay.text = "";
            SetTextAlpha(narrativeTextDisplay, 0f);
        }

        if (birdHintText != null)
        {
            birdHintText.text = "SPACE to fly up, S to land, R to land on planets";
            SetTextAlpha(birdHintText, 0f);
            birdHintText.gameObject.SetActive(false);
        }

        SetImageAlpha(fadeScreen, 0f); // Start with screen visible
    }

    void Update()
    {
        if (transitioning || cameraController == null) return;

    // ONLY RUN IF IN DOG MODE
    if (cameraController.CurrentMode != UmweltCameraController.Mode.Dog) return;
    
        if (transitioning) return;

        float dist = Vector3.Distance(player.position, interactionZone.transform.position);

        if (dist <= interactionRadius)
        {
            if (!inZone)
            {
                inZone = true;
                if (interactionHintText != null)
                    interactionHintText.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(DoTransitionToBirdMode());
            }
        }
        else if (inZone)
        {
            inZone = false;
            if (interactionHintText != null)
                interactionHintText.gameObject.SetActive(false);
        }
    }

    IEnumerator DoTransitionToBirdMode()
    {
        transitioning = true;

        if (interactionHintText != null)
            interactionHintText.gameObject.SetActive(false);

        // Fade to black
        yield return StartCoroutine(FadeImage(fadeScreen, 0f, 1f, fadeToBlackDuration));

        // Show each narrative text
        for (int i = 0; i < narrativeTexts.Count; i++)
        {
            narrativeTextDisplay.text = narrativeTexts[i];
            yield return StartCoroutine(FadeText(narrativeTextDisplay, 0f, 1f, textFadeTime));
            yield return new WaitForSeconds(textHoldTime);
            yield return StartCoroutine(FadeText(narrativeTextDisplay, 1f, 0f, textFadeTime));
        }

        // Switch to bird mode
        EnableBirdMode();

        // Fade screen back in
        yield return StartCoroutine(FadeImage(fadeScreen, 1f, 0f, fadeFromBlackDuration));

        // Show bird hint with fade
        if (birdHintText != null)
        {
            birdHintText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeText(birdHintText, 0f, 1f, textFadeTime));
            yield return new WaitForSeconds(birdHintDuration);
            yield return StartCoroutine(FadeText(birdHintText, 1f, 0f, textFadeTime));
            birdHintText.gameObject.SetActive(false);
        }

        transitioning = false;
    }

    void EnableBirdMode()
    {
        Debug.Log("Switched to Bird Mode!");
        UmweltCameraController controller = FindObjectOfType<UmweltCameraController>();
        if (controller != null)
        {
            controller.SetMode(UmweltCameraController.Mode.Bird);
        }
    }

    IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            img.color = new Color(c.r, c.g, c.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        img.color = new Color(c.r, c.g, c.b, to);
    }

    IEnumerator FadeText(TextMeshProUGUI txt, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = txt.color;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            txt.color = new Color(c.r, c.g, c.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        txt.color = new Color(c.r, c.g, c.b, to);
    }

    void SetTextAlpha(TextMeshProUGUI txt, float a)
    {
        Color c = txt.color;
        txt.color = new Color(c.r, c.g, c.b, a);
    }

    void SetImageAlpha(Image img, float a)
    {
        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, a);
    }
}
