using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UmweltsManager : MonoBehaviour
{
    public static UmweltsManager Instance; // Singleton instance to access from movement script

    public Button personButton;
    public Button dogButton;
    public Button birdButton;

    public Button cityButton;
    public Button forestButton;
    public Button waterButton;

    public Volume globalVolume;
    public VolumeProfile defaultProfile;
    public VolumeProfile dogProfile;
    public VolumeProfile birdProfile;

    public GameObject dogEffectParticles;
    public GameObject dogViewQuad;
    public GameObject personEffectParticles;
    public GameObject birdEffectParticles;

    public GameObject cityScene;
    public GameObject forestScene;
    public GameObject waterScene;

    private void Awake()
    {
        Instance = this; // Assign singleton instance
    }

    void Start()
    {
        dogViewQuad.SetActive(false);

        if (dogButton != null)
            dogButton.onClick.AddListener(() => ApplyEffect(EffectMode.Dog));

        if (personButton != null)
            personButton.onClick.AddListener(() => ApplyEffect(EffectMode.Person));

        if (birdButton != null)
            birdButton.onClick.AddListener(() => ApplyEffect(EffectMode.Bird));

        ApplyEffect(EffectMode.Person);

        if (cityButton != null)
            cityButton.onClick.AddListener(ShowCity);

        if (forestButton != null)
            forestButton.onClick.AddListener(ShowForest);

        if (waterButton != null)
            waterButton.onClick.AddListener(ShowWater);

        ShowCity();
    }

    public enum EffectMode { Person, Dog, Bird }

    public void ApplyEffect(EffectMode mode)
    {
        Debug.Log($"{mode} Effect Applied");

        switch (mode)
        {
            case EffectMode.Person:
                globalVolume.profile = defaultProfile;
                dogViewQuad.SetActive(false);
                ToggleParticleEffects(personEffectParticles);
                break;
            case EffectMode.Dog:
                globalVolume.profile = dogProfile;
                dogViewQuad.SetActive(true);
                ToggleParticleEffects(dogEffectParticles);
                break;
            case EffectMode.Bird:
                globalVolume.profile = birdProfile;
                dogViewQuad.SetActive(false);
                ToggleParticleEffects(birdEffectParticles);
                break;
        }
    }

    public void ShowCity() => ToggleScenes(cityScene);
    public void ShowForest() => ToggleScenes(forestScene);
    public void ShowWater() => ToggleScenes(waterScene);

    private void ToggleParticleEffects(GameObject activeEffect)
    {
        if (dogEffectParticles != null) dogEffectParticles.SetActive(false);
        if (personEffectParticles != null) personEffectParticles.SetActive(false);
        if (birdEffectParticles != null) birdEffectParticles.SetActive(false);

        if (activeEffect != null) activeEffect.SetActive(true);
    }

    private void ToggleScenes(GameObject activeScene)
    {
        if (forestScene != null) forestScene.SetActive(false);
        if (cityScene != null) cityScene.SetActive(false);
        if (waterScene != null) waterScene.SetActive(false);

        if (activeScene != null) activeScene.SetActive(true);
    }
}
