using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeditationSoundscape : MonoBehaviour
{
    public Vector2 playerPosition = new Vector2(0, 0);
    public List<Vector2> noisePositions;
    public Vector2 dogSnoringPosition;
    public float movementStep = 1f;
    public float soundRange = 10f;
    public Vector2 boundaryMin = new Vector2(-10, -10);
    public Vector2 boundaryMax = new Vector2(10, 10);
    
    public List<AudioSource> noiseAudioSources;
    public AudioSource dogSnoringAudio;

    void Start()
    {
        if (noiseAudioSources == null || noisePositions == null || noiseAudioSources.Count != noisePositions.Count)
        {
            Debug.LogError("Mismatch between noise positions and assigned audio sources! Make sure they are correctly assigned in the Inspector.");
            return;
        }

        // Ensure all audio sources start playing
        foreach (var audio in noiseAudioSources)
        {
            if (audio.clip == null)
            {
                Debug.LogError("A noise source has no audio clip assigned!");
            }
            else
            {
                audio.loop = true;
                audio.Play();
            }
        }

        if (dogSnoringAudio.clip == null)
        {
            Debug.LogError("Dog snoring sound has no audio clip assigned!");
        }
        else
        {
            dogSnoringAudio.loop = true;
            dogSnoringAudio.Play();
        }
    }

    void Update()
    {
        HandleMovement();
        UpdateSoundVolumes();
    }

    void HandleMovement()
    {
        Vector2 moveDirection = Vector2.zero;
        
        if (Input.GetKey(KeyCode.LeftArrow))
            moveDirection.x -= movementStep;
        if (Input.GetKey(KeyCode.RightArrow))
            moveDirection.x += movementStep;
        if (Input.GetKey(KeyCode.UpArrow))
            moveDirection.y += movementStep;
        if (Input.GetKey(KeyCode.DownArrow))
            moveDirection.y -= movementStep;
        
        playerPosition += moveDirection;
        
        // Ensure player stays within boundaries
        playerPosition.x = Mathf.Clamp(playerPosition.x, boundaryMin.x, boundaryMax.x);
        playerPosition.y = Mathf.Clamp(playerPosition.y, boundaryMin.y, boundaryMax.y);
    }

    void UpdateSoundVolumes()
    {
        for (int i = 0; i < noiseAudioSources.Count; i++)
        {
            float distance = Vector2.Distance(playerPosition, noisePositions[i]);
            float volume = Mathf.Clamp01(1 - (distance / soundRange));
            noiseAudioSources[i].volume = volume;
            Debug.Log($"Noise {i}: Distance {distance}, Volume {volume}");
        }
        
        // Adjust dog's snoring volume inversely
        float snoringDistance = Vector2.Distance(playerPosition, dogSnoringPosition);
        float snoringVolume = Mathf.Clamp01(1 - (snoringDistance / soundRange));
        dogSnoringAudio.volume = snoringVolume;
        Debug.Log($"Dog Snoring: Distance {snoringDistance}, Volume {snoringVolume}");
    }
}
