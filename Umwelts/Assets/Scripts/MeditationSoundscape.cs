using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeditationSoundscape : MonoBehaviour
{
    public bool meditation = false;
    public bool dogMode = false;
    public bool test = true; // Enable or disable debug indicators
    public Vector2 playerPosition = new Vector2(0, 0);
    public List<Vector2> noisePositions;
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

    private List<GameObject> noiseIndicators = new List<GameObject>();
    private GameObject dogIndicator;
    private float timeSpentInDogZone = 0f;
    
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
    }

   bool indicatorsCreated = false; // Add this at the class level

    void Initialize()
{
    lightIndicator.gameObject.SetActive(true);
    fadeScreen.gameObject.SetActive(true);

    // Ensure dogIndicator is created even if test is false
    if (dogIndicator == null && dogIndicatorPrefab != null && canvasTransform != null)
    {
        dogIndicator = Instantiate(dogIndicatorPrefab, canvasTransform);
        dogIndicator.SetActive(false); // Hides it immediately after creation
        Debug.Log("dogIndicator instantiated and hidden in Initialize().");
    }
    else if (dogIndicator == null)
    {
        Debug.LogError("dogIndicator is NULL, and dogIndicatorPrefab or canvasTransform might be missing!");
    }
     // Disable Person Mode UI when meditation starts
    DisablePersonUI();

    // Only create other indicators if test mode is enabled
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

    if (cameraController != null)
    {
        Debug.Log("Disabling Person Mode UI because meditation started.");

        if (cameraController.interactionHintText != null)
            cameraController.interactionHintText.SetActive(false);
    }
    else
    {
        Debug.LogError("UmweltCameraController not found! Make sure it exists in the scene.");
    }
}

    void InitializeAudio()
    {
        foreach (var audio in noiseAudioSources)
        {
            if (audio.clip == null)
                Debug.LogError("A noise source has no audio clip assigned!");
            else
            {
                audio.loop = true;
                audio.volume = 0;
                audio.Play();
            }
        }

        if (dogSnoringAudio.clip == null)
            Debug.LogError("Dog snoring sound has no audio clip assigned!");
        else
        {
            dogSnoringAudio.loop = true;
            dogSnoringAudio.volume = 0;
            dogSnoringAudio.Play();
        }
    }

    void CreateIndicators()
    {
        foreach (Vector2 pos in noisePositions)
        {
            GameObject noiseIndicator = Instantiate(noiseIndicatorPrefab, canvasTransform);
            RectTransform rectTransform = noiseIndicator.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(pos.x * 50f, pos.y * 50f);
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
            float distance = Vector2.Distance(playerPosition, noisePositions[i]);
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

        // Ensure alpha stays at least 0.5
        float alpha = Mathf.Clamp(1 - (distanceToSnoring / dogSnoringRadius), 0.5f, 1f);

        lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, alpha);
    }

    void UpdateNoiseIndicators()
    {
        for (int i = 0; i < noiseIndicators.Count; i++)
        {
            RectTransform rectTransform = noiseIndicators[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(noisePositions[i].x * 50f, noisePositions[i].y * 50f);
        }
    }

 IEnumerator EndMeditation()
{
    meditation = false;
    Debug.Log("EndMeditation started. Switching to Dog Mode first...");

    // Enable Dog Mode BEFORE fade-out
    dogMode = true;
    EnableDogMode(); 

    float elapsedTime = 0f;

    Image fadeImage = fadeScreen?.GetComponent<Image>();
    Image lightImage = lightIndicator?.GetComponent<Image>();
    Image dogImage = dogIndicator?.GetComponent<Image>();

    List<Image> noiseImages = new List<Image>();
    foreach (var indicator in noiseIndicators)
    {
        if (indicator != null)
        {
            noiseImages.Add(indicator.GetComponent<Image>());
        }
        else
        {
            Debug.LogError("A noiseIndicator is NULL in EndMeditation!");
        }
    }

    Debug.Log("Dog Mode activated. Starting fade-out...");

    // Fade out effect AFTER switching to Dog Mode
    while (elapsedTime < fadeDuration)
    {
        float fadeAmount = 1 - (elapsedTime / fadeDuration);

        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, fadeAmount);
        if (lightImage != null) lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, fadeAmount * 0.5f);
        if (dogImage != null) dogImage.color = new Color(dogImage.color.r, dogImage.color.g, dogImage.color.b, fadeAmount);

        foreach (var noiseImage in noiseImages)
        {
            if (noiseImage != null) noiseImage.color = new Color(noiseImage.color.r, noiseImage.color.g, noiseImage.color.b, fadeAmount);
        }

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    Debug.Log("Fading complete.");

    // Hide UI elements completely
    if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    if (lightImage != null) lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, 0);
    if (dogIndicator != null) dogIndicator.SetActive(false);

    foreach (var noiseImage in noiseImages)
    {
        if (noiseImage != null) noiseImage.color = new Color(noiseImage.color.r, noiseImage.color.g, noiseImage.color.b, 0);
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
        {
            characterController.enabled = true; // Reactivate character movement
        }

        // Ensure SetMode(Dog) is only called when everything is initialized
        if (cameraController.dogSpawnPoint != null)
        {
            cameraController.SetMode(UmweltCameraController.Mode.Dog);
            Debug.Log("Switched to Dog Mode");
        }
        else
        {
            Debug.LogError("Dog Spawn Point is NULL! Assign it in the Unity Inspector.");
        }
    }
    else
    {
        Debug.LogError("UmweltCameraController not found! Make sure it exists in the scene.");
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