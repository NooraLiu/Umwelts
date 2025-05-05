using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetInteraction : MonoBehaviour
{
    public Transform player;
    public float interactionRadius = 10f;

    public List<string> landingNarrativeTexts;
    public TextMeshProUGUI narrativeText;
    public TextMeshProUGUI landingHintText;

    public float textFadeTime = 0.5f;
    public float textHoldTime = 2f;

    private bool playedOnce = false;
    private bool inRange = false;

    void Start()
    {
        if (landingHintText != null)
        {
            landingHintText.gameObject.SetActive(false);
        }

        if (narrativeText != null)
        {
            narrativeText.text = "";
            SetTextAlpha(narrativeText, 0f);
        }
    }

    void Update()
    {
        if (playedOnce || player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool nowInRange = distance <= interactionRadius;

        if (nowInRange && !inRange)
        {
            inRange = true;
            if (landingHintText != null)
                landingHintText.gameObject.SetActive(true);
        }
        else if (!nowInRange && inRange)
        {
            inRange = false;
            if (landingHintText != null)
                landingHintText.gameObject.SetActive(false);
        }

        if (inRange && Input.GetKeyDown(KeyCode.R))
        {
            if (landingHintText != null)
                landingHintText.gameObject.SetActive(false);

            TriggerLandingNarrative();
        }
    }

    public void TriggerLandingNarrative()
    {
        if (playedOnce) return;
        playedOnce = true;
        StartCoroutine(PlayLandingSequence());
    }

    IEnumerator PlayLandingSequence()
    {
        foreach (string line in landingNarrativeTexts)
        {
            narrativeText.text = line;
            yield return StartCoroutine(FadeText(narrativeText, 0f, 1f, textFadeTime));
            yield return new WaitForSeconds(textHoldTime);
            yield return StartCoroutine(FadeText(narrativeText, 1f, 0f, textFadeTime));
        }
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
}
