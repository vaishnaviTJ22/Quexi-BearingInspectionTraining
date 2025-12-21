using UnityEngine;
using UnityEngine.UI;

public class UIFlowManager : MonoBehaviour
{
    [Header("Welcome Screen")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private Button startButton;
    [SerializeField] private AudioClip welcomeAudioClip;

    [Header("Info Screen")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button closeInfoButton;

    [Header("Main UI")]
    [SerializeField] private GameObject explodedViewUI;

    [Header("Dummy Bearing")]
    [SerializeField] private GameObject dummyBearing;
    [SerializeField] private GameObject originalBearing;

    [Header("Audio Settings")]
    [SerializeField] private bool autoPlayWelcomeAudio = true;
    [SerializeField] private bool stopAudioOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Start()
    {
        dummyBearing.SetActive(true);
        originalBearing.SetActive(false);
        InitializeWelcomeScreen();
        SetupButtonListeners();

        if (autoPlayWelcomeAudio)
        {
            PlayWelcomeAudio();
        }
    }

    void OnDestroy()
    {
        RemoveButtonListeners();
    }

    private void InitializeWelcomeScreen()
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (explodedViewUI != null)
        {
            explodedViewUI.SetActive(false);
        }

        if (showDebugLogs)
            Debug.Log("UI Flow initialized - Welcome screen active");
    }

    private void SetupButtonListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.AddListener(ShowInfoPanel);
        }

        if (closeInfoButton != null)
        {
            closeInfoButton.onClick.AddListener(HideInfoPanel);
        }
    }

    private void RemoveButtonListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveListener(ShowInfoPanel);
        }

        if (closeInfoButton != null)
        {
            closeInfoButton.onClick.RemoveListener(HideInfoPanel);
        }
    }

    private void PlayWelcomeAudio()
    {
        if (welcomeAudioClip != null && audioSource != null)
        {
            audioSource.clip = welcomeAudioClip;
            audioSource.Play();

            if (showDebugLogs)
                Debug.Log($"Playing welcome audio - Duration: {welcomeAudioClip.length:F1}s");
        }
        else if (showDebugLogs && welcomeAudioClip == null)
        {
            Debug.LogWarning("UIFlowManager: No welcome audio clip assigned");
        }
    }

    public void StopWelcomeAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();

            if (showDebugLogs)
                Debug.Log("Welcome audio stopped");
        }
    }

    private void OnStartButtonClicked()
    {
        if (stopAudioOnStart)
        {
            StopWelcomeAudio();
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }

        if (explodedViewUI != null)
        {
            explodedViewUI.SetActive(true);
        }

        dummyBearing.SetActive(false);
        originalBearing.SetActive(true);

        if (showDebugLogs)
            Debug.Log("Welcome screen closed - Main UI enabled");
    }

    public void ShowInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        if (showDebugLogs)
            Debug.Log("Info panel displayed");
    }

    public void HideInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (showDebugLogs)
            Debug.Log("Info panel hidden");
    }

    public void ShowWelcomeScreen()
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
        }

        if (explodedViewUI != null)
        {
            explodedViewUI.SetActive(false);
        }

        if (autoPlayWelcomeAudio)
        {
            PlayWelcomeAudio();
        }
    }

    public void ResetToWelcome()
    {
        InitializeWelcomeScreen();

        if (autoPlayWelcomeAudio)
        {
            PlayWelcomeAudio();
        }
    }

    public bool IsWelcomeAudioPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public AudioClip GetWelcomeAudioClip()
    {
        return welcomeAudioClip;
    }
}
