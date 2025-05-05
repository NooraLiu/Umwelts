using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeTransitionController : MonoBehaviour
{
    [Header("References")]
    public UmweltCameraController cameraController;
    public Transform player;
    public ParticleSystem interactionZone;

    [Header("UI Elements")]
    public TextMeshProUGUI interactionHintText;
    public TextMeshProUGUI narrativeTextDisplay;
    public TextMeshProUGUI birdHintText;
    public Image fadeScreen;

    [Header("Narrative Settings")]
    public List<string> narrativeTexts;
    public float textFadeTime = 0.5f;
    public float textHoldTime = 2f;
    public float fadeToBlackDuration = 1f;
    public float fadeFromBlackDuration = 1.5f;
    public float birdHintDuration = 3f;

    [Header("Interaction Settings")]
    public float interactionRadius = 3f;

    private bool inZone = false;
    private bool transitioning = false;

    private Vector3 frozenPosition;

    void Start()
    {
        if (interactionHintText != null) interactionHintText.gameObject.SetActive(false);
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

        SetImageAlpha(fadeScreen, 0f);
    }

    void Update()
    {
        if (transitioning || cameraController == null) return;
        if (cameraController.CurrentMode != UmweltCameraController.Mode.Dog) return;

        float dist = Vector3.Distance(player.position, interactionZone.transform.position);

        if (dist <= interactionRadius)
        {
            if (!inZone)
            {
                inZone = true;
                interactionHintText?.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(DoTransitionToBirdMode());
            }
        }
        else if (inZone)
        {
            inZone = false;
            interactionHintText?.gameObject.SetActive(false);
        }
    }

    IEnumerator DoTransitionToBirdMode()
    {
        transitioning = true;
        interactionHintText?.gameObject.SetActive(false);

        // Freeze position but allow camera to rotate
        frozenPosition = player.position;
        StartCoroutine(LockPlayerPosition());

        // Immediately fade to 20% black
        SetImageAlpha(fadeScreen, 0.2f);

        // Narrative sequence
        foreach (var line in narrativeTexts)
        {
            narrativeTextDisplay.text = line;
            yield return StartCoroutine(FadeText(narrativeTextDisplay, 0f, 1f, textFadeTime));
            yield return new WaitForSeconds(textHoldTime);
            yield return StartCoroutine(FadeText(narrativeTextDisplay, 1f, 0f, textFadeTime));
        }

        // Fade to full black at the end
        yield return StartCoroutine(FadeImage(fadeScreen, 0.2f, 1f, fadeToBlackDuration));

        // Switch to bird mode
        cameraController.SetMode(UmweltCameraController.Mode.Bird);
        transitioning = false;

        // Fade out
        yield return StartCoroutine(FadeImage(fadeScreen, 1f, 0f, fadeFromBlackDuration));

        // Bird hint fade in/out
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

    IEnumerator LockPlayerPosition()
    {
        while (transitioning)
        {
            player.position = frozenPosition;
            yield return null;
        }
    }

    #region Fade Helpers
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
    #endregion
}
