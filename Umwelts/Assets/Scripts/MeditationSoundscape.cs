using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class NoiseTextData
{
    public Vector2 position;
    public List<string> texts;
}

public class MeditationSoundscape : MonoBehaviour
{
    public bool meditation = false;
    public bool dogMode = false;
    public bool test = true;
    public Vector2 playerPosition = new Vector2(0, 0);
    public List<NoiseTextData> noiseTextDataList;
    public Vector2 dogSnoringPosition;
    public float movementStep = 1f;
    public float soundRange = 10f;
    public float dogSnoringRadius = 2f;
    public float timeToTriggerDogMode = 5f;
    public float fadeDuration = 2f;
    public float soundFadeDuration = 2f;
    public Vector2 boundaryMin = new Vector2(-10, -10);
    public Vector2 boundaryMax = new Vector2(10, 10);
    public List<AudioSource> noiseAudioSources;
    public AudioSource dogSnoringAudio;
    public Image fadeScreen;
    public GameObject lightIndicator;
    public GameObject noiseIndicatorPrefab;
    public GameObject dogIndicatorPrefab;
    public Transform canvasTransform;
    public TextMeshProUGUI noiseTextDisplay;

    private List<GameObject> noiseIndicators = new List<GameObject>();
    private GameObject dogIndicator;
    private float timeSpentInDogZone = 0f;

    private int currentNoiseIndex = -1;
    private float textDisplayTimer = 0f;
    private int currentTextIndex = 0;
    private Coroutine textRoutine;

    void Start()
    {
        InitializeAudio();
    }

    void Update()
    {
        if (!meditation) return;

        Initialize();
        HandleMovement();
        UpdateSoundVolumes();
        CheckDogSnoringZone();
        if (test) UpdateNoiseIndicators();
        UpdateLightIndicatorAlpha();
        UpdateNoiseText();
    }

    bool indicatorsCreated = false;

    void Initialize()
    {
        lightIndicator.gameObject.SetActive(true);
        fadeScreen.gameObject.SetActive(true);

        if (dogIndicator == null && dogIndicatorPrefab != null && canvasTransform != null)
        {
            dogIndicator = Instantiate(dogIndicatorPrefab, canvasTransform);
            dogIndicator.SetActive(false);
        }

        DisablePersonUI();

        if (test && !indicatorsCreated)
        {
            CreateIndicators();
            indicatorsCreated = true;
        }

        SetInitialLightAlpha();
    }

    void DisablePersonUI()
    {
        UmweltCameraController cameraController = FindObjectOfType<UmweltCameraController>();

        if (cameraController != null && cameraController.interactionHintText != null)
        {
            cameraController.interactionHintText.SetActive(false);
        }
    }

    void InitializeAudio()
    {
        foreach (var audio in noiseAudioSources)
        {
            if (audio.clip != null)
            {
                audio.loop = true;
                audio.volume = 0;
                audio.Play();
            }
        }

        if (dogSnoringAudio.clip != null)
        {
            dogSnoringAudio.loop = true;
            dogSnoringAudio.volume = 0;
            dogSnoringAudio.Play();
        }
    }

    void CreateIndicators()
    {
        foreach (var data in noiseTextDataList)
        {
            GameObject noiseIndicator = Instantiate(noiseIndicatorPrefab, canvasTransform);
            RectTransform rectTransform = noiseIndicator.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(data.position.x * 50f, data.position.y * 50f);
            noiseIndicators.Add(noiseIndicator);
        }

        dogIndicator = Instantiate(dogIndicatorPrefab, canvasTransform);
        RectTransform dogTransform = dogIndicator.GetComponent<RectTransform>();
        dogTransform.anchoredPosition = new Vector2(dogSnoringPosition.x * 50f, dogSnoringPosition.y * 50f);
    }

    void SetInitialLightAlpha()
    {
        Image lightImage = lightIndicator.GetComponent<Image>();
        lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, 0.5f);
    }

    void HandleMovement()
    {
        Vector2 moveDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= movementStep;
        if (Input.GetKey(KeyCode.D)) moveDirection.x += movementStep;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += movementStep;
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= movementStep;

        playerPosition += moveDirection;
        playerPosition.x = Mathf.Clamp(playerPosition.x, boundaryMin.x, boundaryMax.x);
        playerPosition.y = Mathf.Clamp(playerPosition.y, boundaryMin.y, boundaryMax.y);

        if (lightIndicator != null)
        {
            RectTransform lightTransform = lightIndicator.GetComponent<RectTransform>();
            lightTransform.anchoredPosition = new Vector2(playerPosition.x * 50f, playerPosition.y * 50f);
        }
    }

    void UpdateSoundVolumes()
    {
        float snoringDistance = Vector2.Distance(playerPosition, dogSnoringPosition);
        float snoringVolume = Mathf.Clamp01(1 - (snoringDistance / soundRange));
        dogSnoringAudio.volume = snoringVolume;

        for (int i = 0; i < noiseAudioSources.Count; i++)
        {
            float distance = Vector2.Distance(playerPosition, noiseTextDataList[i].position);
            noiseAudioSources[i].volume = (snoringDistance <= dogSnoringRadius) ? 0 : Mathf.Clamp01(1 - (distance / soundRange));
        }
    }

    void CheckDogSnoringZone()
    {
        float distanceToSnoring = Vector2.Distance(playerPosition, dogSnoringPosition);
        if (distanceToSnoring <= dogSnoringRadius)
        {
            timeSpentInDogZone += Time.deltaTime;
            if (timeSpentInDogZone >= timeToTriggerDogMode)
                StartCoroutine(EndMeditation());
        }
        else timeSpentInDogZone = 0f;
    }

    void UpdateLightIndicatorAlpha()
    {
        float distanceToSnoring = Vector2.Distance(playerPosition, dogSnoringPosition);
        Image lightImage = lightIndicator.GetComponent<Image>();
        float alpha = Mathf.Clamp(1 - (distanceToSnoring / dogSnoringRadius), 0.5f, 1f);
        lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, alpha);
    }

    void UpdateNoiseIndicators()
    {
        for (int i = 0; i < noiseIndicators.Count; i++)
        {
            RectTransform rectTransform = noiseIndicators[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(noiseTextDataList[i].position.x * 50f, noiseTextDataList[i].position.y * 50f);
        }
    }

    void UpdateNoiseText()
    {
        if (dogMode) return; 
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < noiseTextDataList.Count; i++)
        {
            float distance = Vector2.Distance(playerPosition, noiseTextDataList[i].position);
            if (distance < soundRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex != -1)
        {
            if (nearestIndex != currentNoiseIndex)
            {
                currentNoiseIndex = nearestIndex;
                currentTextIndex = 0;
                if (textRoutine != null) StopCoroutine(textRoutine);
                textRoutine = StartCoroutine(ShowTextsSequentially(noiseTextDataList[currentNoiseIndex].texts));
            }
        }
        else
        {
            currentNoiseIndex = -1;
            if (textRoutine != null) StopCoroutine(textRoutine);
            StartCoroutine(FadeOutText());
        }
    }

    IEnumerator ShowTextsSequentially(List<string> texts)
    {
        while (true)
        {
            noiseTextDisplay.text = texts[currentTextIndex];
            yield return StartCoroutine(FadeText(0f, 1f, 0.5f));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FadeText(1f, 0f, 0.5f));
            yield return new WaitForSeconds(0.3f);
            currentTextIndex = (currentTextIndex + 1) % texts.Count;
        }
    }

    IEnumerator FadeText(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color color = noiseTextDisplay.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            noiseTextDisplay.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        noiseTextDisplay.color = new Color(color.r, color.g, color.b, to);
    }

    IEnumerator FadeOutText()
    {
        yield return StartCoroutine(FadeText(noiseTextDisplay.color.a, 0f, 0.5f));
    }

    IEnumerator EndMeditation()
    {
        meditation = false;
        dogMode = true;
         if (textRoutine != null) StopCoroutine(textRoutine);
    textRoutine = null;

    noiseTextDisplay.text = "";
    noiseTextDisplay.color = new Color(
        noiseTextDisplay.color.r,
        noiseTextDisplay.color.g,
        noiseTextDisplay.color.b,
        0f // set alpha to 0 to fully hide
    );
        EnableDogMode();

        float elapsedTime = 0f;

        Image fadeImage = fadeScreen?.GetComponent<Image>();
        Image lightImage = lightIndicator?.GetComponent<Image>();
        Image dogImage = dogIndicator?.GetComponent<Image>();
        List<Image> noiseImages = new List<Image>();

        foreach (var indicator in noiseIndicators)
        {
            if (indicator != null)
                noiseImages.Add(indicator.GetComponent<Image>());
        }

        while (elapsedTime < fadeDuration)
        {
            float fadeAmount = 1 - (elapsedTime / fadeDuration);

            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, fadeAmount);
            if (lightImage != null) lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, fadeAmount * 0.5f);
            if (dogImage != null) dogImage.color = new Color(dogImage.color.r, dogImage.color.g, dogImage.color.b, fadeAmount);

            foreach (var noiseImage in noiseImages)
            {
                if (noiseImage != null)
                    noiseImage.color = new Color(noiseImage.color.r, noiseImage.color.g, noiseImage.color.b, fadeAmount);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
        if (lightImage != null) lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, 0);
        if (dogIndicator != null) dogIndicator.SetActive(false);

        foreach (var noiseImage in noiseImages)
        {
            if (noiseImage != null)
                noiseImage.color = new Color(noiseImage.color.r, noiseImage.color.g, noiseImage.color.b, 0);
        }

        StartCoroutine(FadeOutDogSound());
    }

    void EnableDogMode()
    {
        UmweltCameraController cameraController = FindObjectOfType<UmweltCameraController>();

        if (cameraController != null)
        {
            CharacterController characterController = cameraController.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = true;

            if (cameraController.dogSpawnPoint != null)
                cameraController.SetMode(UmweltCameraController.Mode.Dog);
        }
    }

    IEnumerator FadeOutDogSound()
    {
        float elapsedTime = 0f;
        float initialVolume = dogSnoringAudio.volume;
        while (elapsedTime < soundFadeDuration)
        {
            dogSnoringAudio.volume = Mathf.Lerp(initialVolume, 0, elapsedTime / soundFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dogSnoringAudio.volume = 0;
    }
}
