using Lean.Localization;
using UnityEngine;

public class PartAudioManager : MonoBehaviour
{
    public static PartAudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool autoCreateAudioSource = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private EnginePartData currentPlayingPart;
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

    public void OnPartGrabbed(EnginePartData partData)
    {
        if (partData == null || audioSource == null)
            return;

        AudioClip clip = GetLocalizedAudio(partData.audioPhrase);

        if (clip == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"PartAudioManager: No audio clip found for phrase '{partData.audioPhrase}'");
            return;
        }

        if (currentPlayingPart == partData && audioSource.isPlaying)
        {
            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Same part grabbed - Continue playing");
            return;
        }

        if (currentPlayingPart != null && showDebugLogs)
        {
            Debug.Log($"PartAudioManager: Switching audio to new part");
        }

        StopCurrentAudio();

        audioSource.clip = clip;
        audioSource.Play();
        currentPlayingPart = partData;
        isPlaying = true;

        string currentLang = LeanLocalization.GetFirstCurrentLanguage();
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Playing audio for phrase '{partData.audioPhrase}' ({currentLang}) - Duration: {clip.length:F1}s");
    }

    private AudioClip GetLocalizedAudio(string audioPhraseName)
    {
        if (string.IsNullOrEmpty(audioPhraseName))
            return null;

        LeanTranslation translation = LeanLocalization.GetTranslation(audioPhraseName);

        if (translation != null && translation.Data is AudioClip audioClip)
        {
            return audioClip;
        }

        return null;
    }

    public void OnPartReleased(EnginePartData partData)
    {
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Part released - Audio continues");
    }

    public void OnPartSnapped(EnginePartData partData)
    {
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Part snapped - Audio continues");
    }

    public void StopCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Stopped audio");

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

    public void PlayLocalizedAudio(string phraseName)
    {
        if (string.IsNullOrEmpty(phraseName))
        {
            StopCurrentAudio();
            return;
        }

        AudioClip clip = GetLocalizedAudio(phraseName);
        if (clip != null)
        {
            PlayClip(clip);
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning($"PartAudioManager: No audio found for phrase '{phraseName}'");
            StopCurrentAudio();
        }
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        StopCurrentAudio();
        
        audioSource.clip = clip;
        audioSource.Play();
        isPlaying = true;
        currentPlayingPart = null; 
        
        if (showDebugLogs)
            Debug.Log($"PartAudioManager: Playing generic clip - Duration: {clip.length:F1}s");
    }

    void Update()
    {
        if (isPlaying && audioSource != null && !audioSource.isPlaying)
        {
            if (showDebugLogs)
                Debug.Log($"PartAudioManager: Audio finished playing");

            isPlaying = false;
            currentPlayingPart = null;
        }
    }
}
