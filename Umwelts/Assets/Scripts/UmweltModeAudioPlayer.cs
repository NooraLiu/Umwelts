using UnityEngine;

public class UmweltModeAudioPlayer : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource personAudioSource;
    public AudioSource dogAudioSource;
    public AudioSource birdAudioSource;

    [Header("References")]
    public UmweltCameraController controller; // Assign in Inspector

    [Header("Options")]
    public bool overrideAudio = false;

    private UmweltCameraController.Mode lastMode;

    void Start()
    {
       if (controller == null)
    {
        Debug.LogWarning("UmweltCameraController not assigned in UmweltModeAudioPlayer.");
        enabled = false;
        return;
    }

    if (!overrideAudio)
    {
        UpdateAudioSources();
    }
    }

    void Update()
    {
        if (overrideAudio || controller == null) return;

        if (controller.CurrentMode != lastMode)
        {
            UpdateAudioSources();
        }
    }

    void UpdateAudioSources()
    {
        lastMode = controller.CurrentMode;

        personAudioSource?.Stop();
        dogAudioSource?.Stop();
        birdAudioSource?.Stop();

        // Play based on current mode
        switch (controller.CurrentMode)
        {
            case UmweltCameraController.Mode.Person:
                personAudioSource?.Play();
                break;
            case UmweltCameraController.Mode.Dog:
                dogAudioSource?.Play();
                break;
            case UmweltCameraController.Mode.Bird:
                birdAudioSource?.Play();
                break;
        }
    }
}
