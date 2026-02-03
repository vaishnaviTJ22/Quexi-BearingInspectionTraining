using Lean.Localization;
using System;
using System.Collections.Generic;
using TamilUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartInfoUIManager : MonoBehaviour
{
    public static PartInfoUIManager Instance { get; private set; }

    [Serializable]
    public struct BearingSetup
    {
        public string name;
        public Toggle toggle;
        public GameObject bearingModel;
        public EnginePartData bearingOverviewData;
    }

    [Header("Bearing Selection Configuration")]
    [SerializeField] private GameObject bearingSelectionPanel;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button openSelectionPanelButton;
    [SerializeField] private List<BearingSetup> bearingOptions;

    [Header("UI References")]
    public GameObject infoPanel;
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI partDescriptionText;
    public Image partImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button changeBearingButton;

    [Header("Settings")]
    [SerializeField] private bool showOnGrab = true;
    [SerializeField] private bool hideOnRelease = false;
    [SerializeField] private bool switchOnDifferentPart = true;
    [SerializeField] private bool showDebugLogs = true;

    private EnginePartData currentDisplayedPart;
    private bool isShowingBearingOverview = false;

    private int pendingSelectionIndex = 0;
    private List<GrabbableEnginePart> currentBearingParts = new List<GrabbableEnginePart>();
    private EngineAnimationController currentAnimationController;

    public int CurrentSelectedIndex { get; private set; } = 0;
    public GameObject CurrentSelectedBearing
    {
        get
        {
            if (bearingOptions != null && CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                return bearingOptions[CurrentSelectedIndex].bearingModel;
            }
            return null;
        }
    }

    public EnginePartData CurrentBearingOverviewData
    {
        get
        {
            if (bearingOptions != null && CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                return bearingOptions[CurrentSelectedIndex].bearingOverviewData;
            }
            return null;
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideInfo);

        if (selectButton != null)
            selectButton.onClick.AddListener(ConfirmSelection);

        if (openSelectionPanelButton != null)
            openSelectionPanelButton.onClick.AddListener(OpenBearingSelectionPanel);

        if (changeBearingButton != null)
            changeBearingButton.onClick.AddListener(OnChangeBearingButtonClicked);

        HideInfo();
    }

    void Start()
    {
        LeanLocalization.OnLocalizationChanged += OnLocalizationChanged;

        InitializeBearingSelection();

        OpenBearingSelectionPanel();
    }

    void OnEnable()
    {
        EventManager.UpdateSelectedBearing += OnBearingChanged;
    }

    void OnDisable()
    {
        EventManager.UpdateSelectedBearing -= OnBearingChanged;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (closeButton != null) closeButton.onClick.RemoveListener(HideInfo);
        if (selectButton != null) selectButton.onClick.RemoveListener(ConfirmSelection);
        if (openSelectionPanelButton != null) openSelectionPanelButton.onClick.RemoveListener(OpenBearingSelectionPanel);
        if (changeBearingButton != null) changeBearingButton.onClick.RemoveListener(OnChangeBearingButtonClicked);

        foreach (var option in bearingOptions)
        {
            if (option.toggle != null)
                option.toggle.onValueChanged.RemoveAllListeners();
        }

        UnsubscribeFromCurrentBearingParts();
        UnsubscribeFromAnimationController();
        LeanLocalization.OnLocalizationChanged -= OnLocalizationChanged;
    }

    private void InitializeBearingSelection()
    {
        for (int i = 0; i < bearingOptions.Count; i++)
        {
            int index = i;
            var option = bearingOptions[i];

            if (option.toggle != null)
            {
                option.toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) pendingSelectionIndex = index;
                });
            }
        }

        foreach (var option in bearingOptions)
        {
            if (option.bearingModel != null)
                option.bearingModel.SetActive(false);
        }

        if (bearingOptions.Count > 0 && bearingOptions[0].toggle != null)
        {
            bearingOptions[0].toggle.isOn = true;
        }
    }

    private void OnBearingChanged(GameObject newBearing)
    {
        if (newBearing == null) return;

        UnsubscribeFromAnimationController();

        currentAnimationController = newBearing.GetComponent<EngineAnimationController>();

        if (currentAnimationController != null)
        {
            SubscribeToAnimationController();
            UpdateChangeBearingButtonState();
        }
    }

    private void SubscribeToAnimationController()
    {
        if (currentAnimationController != null)
        {
            currentAnimationController.onExplodeComplete.AddListener(OnExplodedStateChanged);
            currentAnimationController.onReassembleComplete.AddListener(OnExplodedStateChanged);
        }
    }

    private void UnsubscribeFromAnimationController()
    {
        if (currentAnimationController != null)
        {
            currentAnimationController.onExplodeComplete.RemoveListener(OnExplodedStateChanged);
            currentAnimationController.onReassembleComplete.RemoveListener(OnExplodedStateChanged);
        }
    }

    private void OnExplodedStateChanged()
    {
        UpdateChangeBearingButtonState();
    }

    private void UpdateChangeBearingButtonState()
    {
        if (changeBearingButton == null) return;

        bool isNormalView = currentAnimationController != null && !currentAnimationController.IsExploded();
        changeBearingButton.interactable = isNormalView;
        changeBearingButton.gameObject.SetActive(isNormalView);
        if (showDebugLogs)
            Debug.Log($"PartInfoUI: Change Bearing button {(isNormalView ? "enabled" : "disabled")} (Normal View: {isNormalView})");
    }

    private void OnChangeBearingButtonClicked()
    {
        if (PartAudioManager.Instance != null)
        {
            PartAudioManager.Instance.StopCurrentAudio();
        }

        OpenBearingSelectionPanel();

        if (showDebugLogs)
            Debug.Log("PartInfoUI: Change Bearing button clicked - stopped audio and opened selection panel");
    }

    public void OpenBearingSelectionPanel()
    {
        if (bearingSelectionPanel != null)
        {
            bearingSelectionPanel.SetActive(true);
            HideInfo();

            if (PartAudioManager.Instance != null)
            {
                PartAudioManager.Instance.StopCurrentAudio();
            }

            EventManager.UpdateMenuUIActiveState?.Invoke(false);
        }
    }

    private void ConfirmSelection()
    {
        if (pendingSelectionIndex < 0 || pendingSelectionIndex >= bearingOptions.Count) return;

        CurrentSelectedIndex = pendingSelectionIndex;

        for (int i = 0; i < bearingOptions.Count; i++)
        {
            var option = bearingOptions[i];
            if (option.bearingModel != null)
            {
                bool shouldBeActive = (i == CurrentSelectedIndex);
                if (option.bearingModel.activeSelf != shouldBeActive)
                {
                    option.bearingModel.SetActive(shouldBeActive);
                }
            }
        }

        SubscribeToCurrentBearingParts();

        EventManager.UpdateSelectedBearing?.Invoke(CurrentSelectedBearing);
        EventManager.UpdateMenuUIActiveState?.Invoke(true);

        if (bearingSelectionPanel != null)
        {
            bearingSelectionPanel.SetActive(false);
        }

        ShowBearingOverview();

        if (showDebugLogs)
            Debug.Log($"PartInfoUIManager: Confirmed selection. Index: {CurrentSelectedIndex}, Bearing: {CurrentSelectedBearing?.name}");
    }

    private void SubscribeToCurrentBearingParts()
    {
        UnsubscribeFromCurrentBearingParts();

        if (CurrentSelectedBearing == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("PartInfoUIManager: No current bearing to subscribe to");
            return;
        }

        GrabbableEnginePart[] allPartsInBearing = CurrentSelectedBearing.GetComponentsInChildren<GrabbableEnginePart>(true);

        foreach (var part in allPartsInBearing)
        {
            if (part != null)
            {
                part.onPartGrabbed.AddListener(OnPartGrabbed);
                part.onPartReleased.AddListener(OnPartReleased);
                currentBearingParts.Add(part);
            }
        }

        if (showDebugLogs)
            Debug.Log($"PartInfoUI: Subscribed to {currentBearingParts.Count} parts in bearing '{CurrentSelectedBearing.name}'");
    }

    private void UnsubscribeFromCurrentBearingParts()
    {
        foreach (var part in currentBearingParts)
        {
            if (part != null)
            {
                part.onPartGrabbed.RemoveListener(OnPartGrabbed);
                part.onPartReleased.RemoveListener(OnPartReleased);
            }
        }

        currentBearingParts.Clear();

        if (showDebugLogs)
            Debug.Log("PartInfoUI: Unsubscribed from previous bearing parts");
    }

    private void OnLocalizationChanged()
    {
        if (currentDisplayedPart != null && infoPanel != null && infoPanel.activeSelf)
        {
            ShowPartInfo(currentDisplayedPart);
        }
    }

    private void OnPartGrabbed(EnginePartData partData)
    {
        if (partData == null) return;

        if (bearingSelectionPanel != null && bearingSelectionPanel.activeSelf)
        {
            return;
        }

        isShowingBearingOverview = false;

        if (currentDisplayedPart == partData && infoPanel != null && infoPanel.activeSelf)
        {
            if (showDebugLogs)
                Debug.Log($"PartInfoUI: Same part grabbed, panel already showing");
            return;
        }

        if (switchOnDifferentPart || currentDisplayedPart == null)
        {
            ShowPartInfo(partData);

            if (showDebugLogs)
                Debug.Log($"PartInfoUI: Grabbed part '{partData.partName}', showing info panel");
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
        EnginePartData currentBearingData = CurrentBearingOverviewData;

        if (currentBearingData == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"PartInfoUIManager: No bearing overview data for bearing index {CurrentSelectedIndex}");
            return;
        }

        isShowingBearingOverview = true;
        ShowPartInfo(currentBearingData);

        if (PartAudioManager.Instance != null)
        {
            PartAudioManager.Instance.OnPartGrabbed(currentBearingData);
        }

        if (showDebugLogs)
            Debug.Log($"PartInfoUIManager: Showing overview for bearing: {CurrentSelectedBearing?.name}");
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
                Debug.Log("PartInfoUIManager: Hidden bearing overview");
        }
    }

    public void ShowPartInfo(EnginePartData partData)
    {
        if (partData == null) return;

        currentDisplayedPart = partData;

        var nameLocalizer = partNameText != null ? partNameText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>() : null;
        var descLocalizer = partDescriptionText != null ? partDescriptionText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>() : null;

        if (nameLocalizer != null)
        {
            nameLocalizer.FallbackText = partData.fallbackName;
            nameLocalizer.TranslationName = partData.partName;
            nameLocalizer.UpdateLocalization();
        }
        else if (partNameText != null)
        {
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
            UpdateChangeBearingButtonState();
        }

        if (showDebugLogs)
            Debug.Log($"PartInfoUI: Showing info for '{partData.fallbackName}'");
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
    }

    public void ShowPanel()
    {
        if (infoPanel != null && currentDisplayedPart != null)
        {
            infoPanel.SetActive(true);
            UpdateChangeBearingButtonState();
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
