using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UmweltsManager : MonoBehaviour
{
    public static UmweltsManager Instance; // Singleton instance

    [Header("Post-Processing Effects")]
    public Volume globalVolume;
    public VolumeProfile defaultProfile;
    public VolumeProfile dogProfile;
    public VolumeProfile birdProfile;

    [Header("Dog Vision Overlay")]
    public GameObject dogViewQuad; // Overlay effect for dog vision

    private void Awake()
    {
        Instance = this; // Assign singleton instance
    }

    void Start()
    {
        if (dogViewQuad != null)
            dogViewQuad.SetActive(false);

        ApplyEffect(EffectMode.Person); // Start in Person mode
    }

    public enum EffectMode { Person, Dog, Bird }

    public void ApplyEffect(EffectMode mode)
    {
        Debug.Log($"{mode} Effect Applied");

        switch (mode)
        {
            case EffectMode.Person:
                globalVolume.profile = defaultProfile;
                if (dogViewQuad != null) dogViewQuad.SetActive(false);
                break;

            case EffectMode.Dog:
                globalVolume.profile = dogProfile;
                if (dogViewQuad != null) dogViewQuad.SetActive(true);
                break;

            case EffectMode.Bird:
                globalVolume.profile = birdProfile;
                if (dogViewQuad != null) dogViewQuad.SetActive(false);
                break;
        }
    }
}
