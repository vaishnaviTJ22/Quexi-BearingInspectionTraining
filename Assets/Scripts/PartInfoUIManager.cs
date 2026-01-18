using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lean.Localization;
using TamilUI;

public class PartInfoUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI partDescriptionText;
    public Image partImage;

    [Header("Bearing Overview Data")]
    [SerializeField] private EnginePartData bearingOverviewData;

    [Header("Settings")]
    [SerializeField] private bool showOnGrab = true;
    [SerializeField] private bool hideOnRelease = false;
    [SerializeField] private bool switchOnDifferentPart = true;

    [Header("Optional Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private EnginePartData currentDisplayedPart;
    private bool isShowingBearingOverview = false;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInfo);
        }

        HideInfo();
    }

    void Start()
    {
        SubscribeToAllParts();
        LeanLocalization.OnLocalizationChanged += OnLocalizationChanged;
    }

    void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HideInfo);
        }

        UnsubscribeFromAllParts();
        LeanLocalization.OnLocalizationChanged -= OnLocalizationChanged;
    }

    private void OnLocalizationChanged()
    {
        if (currentDisplayedPart != null && infoPanel != null && infoPanel.activeSelf)
        {
            ShowPartInfo(currentDisplayedPart);
        }
    }

    private void SubscribeToAllParts()
    {
        GrabbableEnginePart[] allParts = FindObjectsByType<GrabbableEnginePart>(FindObjectsSortMode.None);

        foreach (var part in allParts)
        {
            if (part != null)
            {
                part.onPartGrabbed.AddListener(OnPartGrabbed);
                part.onPartReleased.AddListener(OnPartReleased);
            }
        }

        if (showDebugLogs)
            Debug.Log($"PartInfoUI: Subscribed to {allParts.Length} parts");
    }

    private void UnsubscribeFromAllParts()
    {
        GrabbableEnginePart[] allParts = FindObjectsByType<GrabbableEnginePart>(FindObjectsSortMode.None);

        foreach (var part in allParts)
        {
            if (part != null)
            {
                part.onPartGrabbed.RemoveListener(OnPartGrabbed);
                part.onPartReleased.RemoveListener(OnPartReleased);
            }
        }
    }

    private void OnPartGrabbed(EnginePartData partData)
    {
        if (partData == null) return;

        isShowingBearingOverview = false;

        if (currentDisplayedPart == partData && infoPanel != null && infoPanel.activeSelf)
        {
            if (showDebugLogs)
                Debug.Log($"PartInfoUI: Same part - Keep panel open");
            return;
        }

        if (switchOnDifferentPart || currentDisplayedPart == null)
        {
            ShowPartInfo(partData);
        }
    }

    private void OnPartReleased()
    {
        if (hideOnRelease)
        {
            HideInfo();
        }
    }

    public void ShowBearingOverview()
    {
        if (bearingOverviewData == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("PartInfoUI: No bearing overview data assigned!");
            return;
        }

        isShowingBearingOverview = true;
        ShowPartInfo(bearingOverviewData);

        if (PartAudioManager.Instance != null)
        {
            PartAudioManager.Instance.OnPartGrabbed(bearingOverviewData);
        }

        if (showDebugLogs)
            Debug.Log("PartInfoUI: Showing bearing overview");
    }

    public void HideBearingOverview()
    {
        if (isShowingBearingOverview)
        {
            HideInfo();
            isShowingBearingOverview = false;

            if (PartAudioManager.Instance != null)
            {
                PartAudioManager.Instance.StopCurrentAudio();
            }

            if (showDebugLogs)
                Debug.Log("PartInfoUI: Hidden bearing overview");
        }
    }

    public void ShowPartInfo(EnginePartData partData)
    {
        if (partData == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("PartInfoUI: Cannot display null part data");
            return;
        }

        currentDisplayedPart = partData;

        // Get the localized components
        var nameLocalizer = partNameText != null ? partNameText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>() : null;
        var descLocalizer = partDescriptionText != null ? partDescriptionText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>() : null;

        if (nameLocalizer != null)
        {
            // Update the localization component's data
            nameLocalizer.FallbackText = partData.fallbackName;
            nameLocalizer.TranslationName = partData.partName;
            // Force an update - this will call UpdateTranslation which now handles fonts correctly
            nameLocalizer.UpdateLocalization();
        }
        else if (partNameText != null)
        {
            // Fallback if no localization component exists
            partNameText.text = GetLocalizedText(partData.partName, partData.fallbackName);
        }

        if (descLocalizer != null)
        {
            descLocalizer.FallbackText = partData.fallbackDescription;
            descLocalizer.TranslationName = partData.partDescription;
            descLocalizer.UpdateLocalization();
        }
        else if (partDescriptionText != null)
        {
            partDescriptionText.text = GetLocalizedText(partData.partDescription, partData.fallbackDescription);
        }

        if (partImage != null)
        {
            partImage.enabled = false;
        }

        if (showOnGrab && infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        if (showDebugLogs)
            Debug.Log($"PartInfoUI: Displaying info with phrases '{partData.partName}', '{partData.partDescription}'");
    }

    private string GetLocalizedText(string phraseName, string fallback)
    {
        if (string.IsNullOrEmpty(phraseName))
            return fallback;

        LeanTranslation translation = LeanLocalization.GetTranslation(phraseName);

        if (translation != null && translation.Data is string translatedText)
        {
            return translatedText;
        }

        return fallback;
    }

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        currentDisplayedPart = null;
        isShowingBearingOverview = false;

        if (showDebugLogs)
            Debug.Log("PartInfoUI: Panel hidden");
    }

    public void ShowPanel()
    {
        if (infoPanel != null && currentDisplayedPart != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public EnginePartData GetCurrentDisplayedPart()
    {
        return currentDisplayedPart;
    }

    public bool IsShowingBearingOverview()
    {
        return isShowingBearingOverview;
    }
}
