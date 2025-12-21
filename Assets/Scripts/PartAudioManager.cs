using UnityEngine;
using System.Collections.Generic;

public class PartAudioManager : MonoBehaviour
{
    public static PartAudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool autoCreateAudioSource = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private EnginePartData currentPlayingPart;
    private Dictionary<string, AudioClip> partAudioClips = new Dictionary<string, AudioClip>();
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (audioSource == null && autoCreateAudioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;

            if (showDebugLogs)
                Debug.Log("PartAudioManager: Created AudioSource");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterPartAudio(EnginePartData partData, AudioClip audioClip)
    {
        if (partData == null || audioClip == null || string.IsNullOrEmpty(partData.partName))
        {
            if (showDebugLogs)
                Debug.LogWarning("PartAudioManager: Cannot register - missing data or clip");
            return;
        }

        if (!partAudioClips.ContainsKey(partData.partName))
        {
            partAudioClips[partData.partName] = audioClip;

            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Registered audio for '{partData.partName}' - Clip: {audioClip.name}");
        }
    }

    public void OnPartGrabbed(EnginePartData partData)
    {
        if (partData == null || audioSource == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("PartAudioManager: Part or AudioSource is null");
            return;
        }

        if (currentPlayingPart == partData)
        {
            if (isPlaying && audioSource.isPlaying)
            {
                if (showDebugLogs)
                    Debug.Log($"PartAudioManager: Same part '{partData.partName}' grabbed - Continue playing");
                return;
            }
            else if (isPlaying && !audioSource.isPlaying)
            {
                if (showDebugLogs)
                    Debug.Log($"PartAudioManager: Audio finished for '{partData.partName}'");
                isPlaying = false;
                return;
            }
        }

        if (partAudioClips.TryGetValue(partData.partName, out AudioClip clip))
        {
            if (currentPlayingPart != null && showDebugLogs)
            {
                Debug.Log($"PartAudioManager: Switching from '{currentPlayingPart.partName}' to '{partData.partName}'");
            }

            StopCurrentAudio();

            audioSource.clip = clip;
            audioSource.Play();
            currentPlayingPart = partData;
            isPlaying = true;

            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Playing audio for '{partData.partName}' - Duration: {clip.length:F1}s");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"PartAudioManager: No audio clip registered for '{partData.partName}'. Registered parts: {string.Join(", ", partAudioClips.Keys)}");
        }
    }

    public void OnPartReleased(EnginePartData partData)
    {
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Part '{partData?.partName}' released - Audio continues");
    }

    public void OnPartSnapped(EnginePartData partData)
    {
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Part '{partData?.partName}' snapped - Audio continues");
    }

    public void StopCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Stopped audio for '{currentPlayingPart?.partName}'");

            audioSource.Stop();
        }

        currentPlayingPart = null;
        isPlaying = false;
    }

    public bool IsPlayingPartAudio(EnginePartData partData)
    {
        return currentPlayingPart == partData && isPlaying && audioSource != null && audioSource.isPlaying;
    }

    public EnginePartData GetCurrentPlayingPart()
    {
        return currentPlayingPart;
    }

    void Update()
    {
        if (isPlaying && audioSource != null && !audioSource.isPlaying)
        {
            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Audio finished playing for '{currentPlayingPart?.partName}'");

            isPlaying = false;
            currentPlayingPart = null;
        }
    }
}
