using Lean.Localization;
using System;
using System.Collections;
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
        public GameObject bearingModel; // Interaction Model
        public EnginePartData bearingOverviewData;
        public QuizData quizData;
        public InspectionData inspectionData; 
        
        [Header("Learn to Fix")]
        public LearnData learnData;
        public GameObject animationBearingModel; // Animation/Visual Model (Non-interactive)
    }

    [Header("Bearing Selection Configuration")]
    [SerializeField] private GameObject bearingSelectionPanel;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button openSelectionPanelButton;
    [SerializeField] private List<BearingSetup> bearingOptions;

    [Header("UI References - Main")]
    public GameObject infoPanel;
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI partDescriptionText;
    public Image partImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button changeBearingButton;
    [SerializeField] private Button startQuizButton;
    [SerializeField] private Button startLearnButton; // New Learn Button

    [Header("UI References - Quiz")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI quizQuestionText;
    [SerializeField] private Button[] quizOptionButtons;
    [SerializeField] private TextMeshProUGUI[] quizOptionTexts;
    [SerializeField] private GameObject quizResultPanel;
    [SerializeField] private TextMeshProUGUI quizResultText;
    [SerializeField] private Button closeQuizButton;
    [SerializeField] private Button restartQuizButton;

    [Header("UI References - Inspection")]
    [SerializeField] private GameObject inspectionPanel;
    [SerializeField] private Image inspectionImage;
    [SerializeField] private TextMeshProUGUI inspectionDescriptionText;
    [SerializeField] private Button nextInspectionButton;
    [SerializeField] private Button prevInspectionButton;
    [SerializeField] private Button closeInspectionButton;
    [SerializeField] private TextMeshProUGUI inspectionProgressText; 

    [Header("UI References - Learn")]
    [SerializeField] private GameObject learnPanel;
    [SerializeField] private TextMeshProUGUI learnTitleText;
    [SerializeField] private TextMeshProUGUI learnDescriptionText;
    [SerializeField] private Button nextLearnButton;
    [SerializeField] private Button prevLearnButton;
    [SerializeField] private Button closeLearnButton;
    [SerializeField] private TextMeshProUGUI learnProgressText;

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

    // Quiz State
    private QuizData currentQuizData;
    private int currentQuizIndex = 0;
    private int currentScore = 0;
    private bool isQuizActive = false;

    // Inspection State
    private InspectionData currentInspectionData;
    private int currentInspectionIndex = 0;
    private bool isInspectionActive = false;

    // Learn State
    private LearnData currentLearnData;
    private int currentLearnIndex = 0;
    private bool isLearnActive = false;

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
    
    public GameObject CurrentAnimationBearing
    {
        get
        {
            if (bearingOptions != null && CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                return bearingOptions[CurrentSelectedIndex].animationBearingModel;
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

        // Main UI Listeners
        if (closeButton != null) closeButton.onClick.AddListener(HideInfo);
        if (selectButton != null) selectButton.onClick.AddListener(ConfirmSelection);
        if (openSelectionPanelButton != null) openSelectionPanelButton.onClick.AddListener(OpenBearingSelectionPanel);
        if (changeBearingButton != null) changeBearingButton.onClick.AddListener(OnChangeBearingButtonClicked);
        if (startQuizButton != null) startQuizButton.onClick.AddListener(StartQuiz);
        if (startLearnButton != null) startLearnButton.onClick.AddListener(StartLearn);
        
        // Quiz UI Listeners
        if (closeQuizButton != null) closeQuizButton.onClick.AddListener(CloseQuiz);
        if (restartQuizButton != null) restartQuizButton.onClick.AddListener(StartQuiz);

        // Inspection UI Listeners
        if (nextInspectionButton != null) nextInspectionButton.onClick.AddListener(NextInspectionStep);
        if (prevInspectionButton != null) prevInspectionButton.onClick.AddListener(PrevInspectionStep);
        if (closeInspectionButton != null) closeInspectionButton.onClick.AddListener(CloseInspection);

        // Learn UI Listeners
        if (nextLearnButton != null) nextLearnButton.onClick.AddListener(NextLearnStep);
        if (prevLearnButton != null) prevLearnButton.onClick.AddListener(PrevLearnStep);
        if (closeLearnButton != null) closeLearnButton.onClick.AddListener(CloseLearn);

        // Setup Quiz Options
        if (quizOptionButtons != null)
        {
            for (int i = 0; i < quizOptionButtons.Length; i++)
            {
                int index = i;
                if (quizOptionButtons[i] != null)
                {
                    quizOptionButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                }
            }
        }

        HideInfo();
        if (quizPanel != null) quizPanel.SetActive(false);
        if (quizResultPanel != null) quizResultPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        if (learnPanel != null) learnPanel.SetActive(false);
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
        if (startQuizButton != null) startQuizButton.onClick.RemoveListener(StartQuiz);
        if (startLearnButton != null) startLearnButton.onClick.RemoveListener(StartLearn);
        
        if (quizOptionButtons != null)
        {
            foreach (var btn in quizOptionButtons)
            {
                if (btn != null) btn.onClick.RemoveAllListeners();
            }
        }

        if (nextInspectionButton != null) nextInspectionButton.onClick.RemoveListener(NextInspectionStep);
        if (prevInspectionButton != null) prevInspectionButton.onClick.RemoveListener(PrevInspectionStep);
        if (closeInspectionButton != null) closeInspectionButton.onClick.RemoveListener(CloseInspection);
        
        if (nextLearnButton != null) nextLearnButton.onClick.RemoveListener(NextLearnStep);
        if (prevLearnButton != null) prevLearnButton.onClick.RemoveListener(PrevLearnStep);
        if (closeLearnButton != null) closeLearnButton.onClick.RemoveListener(CloseLearn);

        foreach (var option in bearingOptions)
        {
            if (option.toggle != null)
                option.toggle.onValueChanged.RemoveAllListeners();
        }

        UnsubscribeFromCurrentBearingParts();
        UnsubscribeFromAnimationController();
        LeanLocalization.OnLocalizationChanged -= OnLocalizationChanged;
    }

    // --- Bearing Selection Logic ---

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

        // Hide all models
        foreach (var option in bearingOptions)
        {
            if (option.bearingModel != null) option.bearingModel.SetActive(false);
            if (option.animationBearingModel != null) option.animationBearingModel.SetActive(false);
        }

        if (bearingOptions.Count > 0 && bearingOptions[0].toggle != null)
        {
            bearingOptions[0].toggle.isOn = true;
        }
    }

    private void ConfirmSelection()
    {
        if (pendingSelectionIndex < 0 || pendingSelectionIndex >= bearingOptions.Count) return;

        CurrentSelectedIndex = pendingSelectionIndex;
        isLearnActive = false; // Reset learn state on selection

        for (int i = 0; i < bearingOptions.Count; i++)
        {
            var option = bearingOptions[i];
            bool shouldBeActive = (i == CurrentSelectedIndex);
            
            // Interaction Model - Active only if selected AND NOT learning
            if (option.bearingModel != null)
            {
                option.bearingModel.SetActive(shouldBeActive && !isLearnActive);
            }
            
            // Animation Model - Initially hidden
            if (option.animationBearingModel != null)
            {
                option.animationBearingModel.SetActive(false);
            }
        }

        SubscribeToCurrentBearingParts();

        EventManager.UpdateSelectedBearing?.Invoke(CurrentSelectedBearing);
        EventManager.UpdateMenuUIActiveState?.Invoke(true);

        if (bearingSelectionPanel != null)
        {
            bearingSelectionPanel.SetActive(false);
        }
        
        if (learnPanel != null) learnPanel.SetActive(false);

        ShowBearingOverview();

        if (showDebugLogs)
            Debug.Log($"PartInfoUIManager: Confirmed selection. Index: {CurrentSelectedIndex}");
    }

    // --- Learn Logic ---

    private void StartLearn()
    {
        if (CurrentSelectedIndex < 0 || CurrentSelectedIndex >= bearingOptions.Count) return;
        
        currentLearnData = bearingOptions[CurrentSelectedIndex].learnData;
        
        if (currentLearnData == null || currentLearnData.steps == null || currentLearnData.steps.Count == 0)
        {
             if (showDebugLogs) Debug.LogWarning("Learn: No data defined for this bearing.");
             return;
        }

        isLearnActive = true;
        currentLearnIndex = 0;
        
        // Hide other panels
        HideInfo();
        if (quizPanel != null) quizPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        if (learnPanel != null) learnPanel.SetActive(true);

        // Swap Models
        if (CurrentSelectedBearing != null) CurrentSelectedBearing.SetActive(false);
        if (CurrentAnimationBearing != null) CurrentAnimationBearing.SetActive(true);

        UpdateLearnUI();
        EventManager.UpdateMenuUIActiveState?.Invoke(false); // Maybe hide menu interaction?
    }
    IEnumerator DelayOnEnableSelectedBearing()
    {
        CurrentAnimationBearing.SetActive(false);
        yield return new WaitForSeconds(.5f);
        CurrentAnimationBearing.SetActive(true);
        CurrentAnimationBearing.transform.rotation = Quaternion.identity;
    }
    private void NextLearnStep()
    {
        if (!isLearnActive || currentLearnData == null) return;
        if (currentLearnIndex < currentLearnData.steps.Count - 1)
        {
            currentLearnIndex++;
            UpdateLearnUI();
        }
    }

    private void PrevLearnStep()
    {
        if (!isLearnActive || currentLearnData == null) return;
        if (currentLearnIndex > 0)
        {
            currentLearnIndex--;
            UpdateLearnUI();
        }
    }

    private void UpdateLearnUI()
    {
        if (currentLearnData == null || currentLearnIndex < 0 || currentLearnIndex >= currentLearnData.steps.Count) return;

        var step = currentLearnData.steps[currentLearnIndex];

        // Localized Title
        if (learnTitleText != null) 
        {
            var titleLocalizer = learnTitleText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>();
            if (titleLocalizer != null)
            {
                titleLocalizer.FallbackText = step.fallbackTitle;
                titleLocalizer.TranslationName = step.titlePhrase;
                titleLocalizer.UpdateLocalization();
            }
            else
            {
                learnTitleText.text = GetLocalizedText(step.titlePhrase, step.fallbackTitle);
            }
        }

        // Localized Description
        if (learnDescriptionText != null) 
        {
            var descLocalizer = learnDescriptionText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>();
            if (descLocalizer != null)
            {
                descLocalizer.FallbackText = step.fallbackDescription;
                descLocalizer.TranslationName = step.descriptionPhrase;
                descLocalizer.UpdateLocalization();
            }
            else
            {
                learnDescriptionText.text = GetLocalizedText(step.descriptionPhrase, step.fallbackDescription);
            }
        }
        
        if (learnProgressText != null)
        {
             learnProgressText.text = $"{currentLearnIndex + 1} / {currentLearnData.steps.Count}";
        }

        // Play Audio (Localized)
        if (PartAudioManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(step.audioPhrase))
                PartAudioManager.Instance.PlayLocalizedAudio(step.audioPhrase);
            else
                PartAudioManager.Instance.StopCurrentAudio();
        }

        // Play Animation
        if (CurrentAnimationBearing != null && !string.IsNullOrEmpty(step.animationStateName))
        {
            var animator = CurrentAnimationBearing.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(step.animationStateName);
            }
        }

        if (prevLearnButton != null) prevLearnButton.interactable = currentLearnIndex > 0;
        if (nextLearnButton != null) nextLearnButton.interactable = currentLearnIndex < currentLearnData.steps.Count - 1;
    }

    private void CloseLearn()
    {
        isLearnActive = false;
        
        if (learnPanel != null) learnPanel.SetActive(false);
        
        if (PartAudioManager.Instance != null) PartAudioManager.Instance.StopCurrentAudio();

        // Swap Models Back
        if (CurrentAnimationBearing != null)
        {
            CurrentAnimationBearing.SetActive(false);
            //currentAnimationController.transform.rotation = Quaternion.identity;
        }
        if (CurrentSelectedBearing != null) CurrentSelectedBearing.SetActive(true);
        
        EventManager.UpdateMenuUIActiveState?.Invoke(true);

        ShowBearingOverview();
    }


    // --- Inspection Logic ---

    private void InitializeInspection()
    {
        if (CurrentSelectedIndex < 0 || CurrentSelectedIndex >= bearingOptions.Count) return;

        currentInspectionData = bearingOptions[CurrentSelectedIndex].inspectionData;

        // If no data, we don't activate inspection
        if (currentInspectionData == null || currentInspectionData.steps == null || currentInspectionData.steps.Count == 0)
        {
            if (inspectionPanel != null) inspectionPanel.SetActive(false);
            isInspectionActive = false;
            return;
        }

        currentInspectionIndex = 0;
        isInspectionActive = true;
        
        UpdateInspectionUI();
    }

    private void NextInspectionStep()
    {
        if (!isInspectionActive || currentInspectionData == null) return;

        if (currentInspectionIndex < currentInspectionData.steps.Count - 1)
        {
            currentInspectionIndex++;
            UpdateInspectionUI();
        }
    }

    private void PrevInspectionStep()
    {
        if (!isInspectionActive || currentInspectionData == null) return;

        if (currentInspectionIndex > 0)
        {
            currentInspectionIndex--;
            UpdateInspectionUI();
        }
    }

    private void UpdateInspectionUI()
    {
        if (currentInspectionData == null || currentInspectionIndex < 0 || currentInspectionIndex >= currentInspectionData.steps.Count) return;

        var step = currentInspectionData.steps[currentInspectionIndex];

        if (inspectionImage != null)
        {
            inspectionImage.sprite = step.stepImage;
            inspectionImage.enabled = step.stepImage != null;
        }

        // Localized Description
        if (inspectionDescriptionText != null)
        {
            var descLocalizer = inspectionDescriptionText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>();
            if (descLocalizer != null)
            {
                descLocalizer.FallbackText = step.fallbackDescription;
                descLocalizer.TranslationName = step.descriptionPhrase;
                descLocalizer.UpdateLocalization();
            }
            else
            {
                inspectionDescriptionText.text = GetLocalizedText(step.descriptionPhrase, step.fallbackDescription);
            }
        }

        if (inspectionProgressText != null)
        {
            inspectionProgressText.text = $"{currentInspectionIndex + 1} / {currentInspectionData.steps.Count}";
        }
        
        // Play Audio (Localized)
        if (PartAudioManager.Instance != null)
        {
            /*if (!string.IsNullOrEmpty(step.audioPhrase))
                PartAudioManager.Instance.PlayLocalizedAudio(step.audioPhrase);
            else
                PartAudioManager.Instance.StopCurrentAudio();*/
        }

        // Button States
        if (prevInspectionButton != null) prevInspectionButton.interactable = currentInspectionIndex > 0;
        if (nextInspectionButton != null) nextInspectionButton.interactable = currentInspectionIndex < currentInspectionData.steps.Count - 1;
    }

    private void CloseInspection()
    {
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        isInspectionActive = false;
    }

    private void UpdateAdditionalUIState() 
    {
        bool isNormalView = currentAnimationController != null && !currentAnimationController.IsExploded();
        bool allowExtras = isShowingBearingOverview && isNormalView; // Basic condition

        // Quiz Button Logic
        if (startQuizButton != null)
        {
            bool hasQuestions = false;
            if (CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                var setup = bearingOptions[CurrentSelectedIndex];
                hasQuestions = setup.quizData != null && setup.quizData.questions != null && setup.quizData.questions.Count > 0;
            }
            startQuizButton.gameObject.SetActive(allowExtras && hasQuestions && !isLearnActive);
        }

        // Learn Button Logic
        if (startLearnButton != null)
        {
            bool hasLearn = false;
             if (CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                var setup = bearingOptions[CurrentSelectedIndex];
                hasLearn = setup.learnData != null && setup.learnData.steps != null && setup.learnData.steps.Count > 0;
            }
            startLearnButton.gameObject.SetActive(allowExtras && hasLearn && !isLearnActive);
        }

        // Inspection Panel Logic (Auto-Enable)
        if (inspectionPanel != null)
        {
            bool hasInspection = false;
            if (CurrentSelectedIndex >= 0 && CurrentSelectedIndex < bearingOptions.Count)
            {
                var setup = bearingOptions[CurrentSelectedIndex];
                hasInspection = setup.inspectionData != null && setup.inspectionData.steps != null && setup.inspectionData.steps.Count > 0;
            }

            bool shouldShow = allowExtras && hasInspection && !isQuizActive && !isLearnActive; 
            
            if (shouldShow)
            {
                if (!inspectionPanel.activeSelf) 
                {
                    inspectionPanel.SetActive(true);
                    InitializeInspection(); // Reset to start
                }
            }
            else
            {
                inspectionPanel.SetActive(false);
            }
        }
    }


    // --- Quiz Logic ---

    private void StartQuiz()
    {
        if (CurrentSelectedIndex < 0 || CurrentSelectedIndex >= bearingOptions.Count) return;

        currentQuizData = bearingOptions[CurrentSelectedIndex].quizData;

        if (currentQuizData == null || currentQuizData.questions == null || currentQuizData.questions.Count == 0)
        {
            if (showDebugLogs) Debug.LogWarning("Quiz: No questions defined for this bearing.");
            return;
        }

        currentQuizIndex = 0;
        currentScore = 0;
        isQuizActive = true;

        if (infoPanel != null) infoPanel.SetActive(false);
        if (quizResultPanel != null) quizResultPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false); 
        if (learnPanel != null) learnPanel.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(true);

        EventManager.UpdateMenuUIActiveState?.Invoke(false);

        DisplayQuestion();
    }

    private void DisplayQuestion()
    {
        if (currentQuizData == null || currentQuizIndex >= currentQuizData.questions.Count)
        {
            EndQuiz();
            return;
        }

        var q = currentQuizData.questions[currentQuizIndex];

        // Localized Question
        if (quizQuestionText != null) 
        {
            var qLocalizer = quizQuestionText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>();
            if (qLocalizer != null)
            {
                qLocalizer.FallbackText = q.fallbackQuestion;
                qLocalizer.TranslationName = q.questionPhrase;
                qLocalizer.UpdateLocalization();
            }
            else
            {
                quizQuestionText.text = GetLocalizedText(q.questionPhrase, q.fallbackQuestion); 
            }
        }
        
        // Play Audio (Localized)
        if (PartAudioManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(q.audioPhrase))
                 PartAudioManager.Instance.PlayLocalizedAudio(q.audioPhrase);
            else
                 PartAudioManager.Instance.StopCurrentAudio();
        }

        if (quizOptionButtons != null)
        {
            for (int i = 0; i < quizOptionButtons.Length; i++)
            {
                if (q.optionPhrases != null && i < q.optionPhrases.Length)
                {
                    quizOptionButtons[i].gameObject.SetActive(true);
                    quizOptionButtons[i].interactable = true; 
                    
                    if (quizOptionTexts != null && i < quizOptionTexts.Length)
                    {
                        var optLocalizer = quizOptionTexts[i].GetComponent<LeanLocalizedTamilTextMeshProUGUI>();
                        string phrase = q.optionPhrases[i];
                        string fallback = (q.fallbackOptions != null && i < q.fallbackOptions.Length) ? q.fallbackOptions[i] : "";

                        if (optLocalizer != null)
                        {
                            optLocalizer.FallbackText = fallback;
                            optLocalizer.TranslationName = phrase;
                            optLocalizer.UpdateLocalization();
                        }
                        else
                        {
                             quizOptionTexts[i].text = GetLocalizedText(phrase, fallback);
                        }
                    }
                }
                else
                {
                    quizOptionButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnAnswerSelected(int selectedOptionIndex)
    {
        if (!isQuizActive || currentQuizData == null) return;

        var q = currentQuizData.questions[currentQuizIndex];

        if (selectedOptionIndex == q.correctOptionIndex)
        {
            currentScore++;
        }

        currentQuizIndex++;
        DisplayQuestion();
    }

    private void EndQuiz()
    {
        isQuizActive = false;
        
        if (quizPanel != null) quizPanel.SetActive(false);
        if (quizResultPanel != null)
        {
            quizResultPanel.SetActive(true);
            if (quizResultText != null)
            {
                string count = currentQuizData != null && currentQuizData.questions != null ? currentQuizData.questions.Count.ToString() : "?";
                quizResultText.text = $"Quiz Completed!\nScore: {currentScore} / {count}";
            }
        }
    }

    private void CloseQuiz()
    {
        isQuizActive = false;
        if (quizPanel != null) quizPanel.SetActive(false);
        if (quizResultPanel != null) quizResultPanel.SetActive(false);
        
        EventManager.UpdateMenuUIActiveState?.Invoke(true);

        ShowBearingOverview(); 
    }

    // --- Existing Logic Updated ---

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
        UpdateAdditionalUIState(); // Update Quiz Button / Inspection Panel visibility
    }

    private void UpdateChangeBearingButtonState()
    {
        if (changeBearingButton == null) return;
        bool isNormalView = currentAnimationController != null && !currentAnimationController.IsExploded();
        changeBearingButton.interactable = isNormalView;
        changeBearingButton.gameObject.SetActive(isNormalView);
    }

    private void OnChangeBearingButtonClicked()
    {
        if (PartAudioManager.Instance != null) PartAudioManager.Instance.StopCurrentAudio();
        OpenBearingSelectionPanel();
    }

    public void OpenBearingSelectionPanel()
    {
        // 1. Activate Selection Panel
        if (bearingSelectionPanel != null)
        {
            bearingSelectionPanel.SetActive(true);
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("PartInfoUIManager: Bearing Selection Panel is not assigned!");
        }

        // 2. Hide Everything Else
        HideInfo();
        if (inspectionPanel != null) inspectionPanel.SetActive(false); 
        if (learnPanel != null) learnPanel.SetActive(false); 
        if (quizPanel != null) quizPanel.SetActive(false);
        if (quizResultPanel != null) quizResultPanel.SetActive(false);

        // 3. Reset Models
        if (CurrentSelectedBearing != null) CurrentSelectedBearing.SetActive(false); // Hide model during selection
        if (CurrentAnimationBearing != null) CurrentAnimationBearing.SetActive(false); 

        // 4. Reset States
        isLearnActive = false;
        isQuizActive = false;
        isInspectionActive = false;
        
        if (PartAudioManager.Instance != null) PartAudioManager.Instance.StopCurrentAudio();
        
        // 5. Update Menu
        EventManager.UpdateMenuUIActiveState?.Invoke(false);
    }

    private void SubscribeToCurrentBearingParts()
    {
        UnsubscribeFromCurrentBearingParts();

        if (CurrentSelectedBearing == null) return;

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
        if (bearingSelectionPanel != null && bearingSelectionPanel.activeSelf) return;

        isShowingBearingOverview = false;

        if (currentDisplayedPart == partData && infoPanel != null && infoPanel.activeSelf) return;

        if (switchOnDifferentPart || currentDisplayedPart == null)
        {
            ShowPartInfo(partData);
        }
    }

    private void OnPartReleased()
    {
        if (hideOnRelease) HideInfo();
    }

    public void ShowBearingOverview()
    {
        EnginePartData currentBearingData = CurrentBearingOverviewData;
        if (currentBearingData == null) return;

        isShowingBearingOverview = true;
        ShowPartInfo(currentBearingData); // This calls UpdateAdditionalUIState

        if (PartAudioManager.Instance != null)
            PartAudioManager.Instance.OnPartGrabbed(currentBearingData);
    }

    public void HideBearingOverview()
    {
        if (isShowingBearingOverview)
        {
            HideInfo();
            isShowingBearingOverview = false;
            
            if (PartAudioManager.Instance != null)
                PartAudioManager.Instance.StopCurrentAudio();
        }
    }

    public void ShowPartInfo(EnginePartData partData)
    {
        if (partData == null) return;

        currentDisplayedPart = partData;
        
        UpdateAdditionalUIState(); 

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

        if (partImage != null) partImage.enabled = false;

        if (showOnGrab && infoPanel != null)
        {
            infoPanel.SetActive(true);
            UpdateChangeBearingButtonState();
        }
    }

    private string GetLocalizedText(string phraseName, string fallback)
    {
        if (string.IsNullOrEmpty(phraseName)) return fallback;
        LeanTranslation translation = LeanLocalization.GetTranslation(phraseName);
        if (translation != null && translation.Data is string translatedText) return translatedText;
        return fallback;
    }

    public void HideInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false); // Hide inspection too
        currentDisplayedPart = null;
        isShowingBearingOverview = false;
    }
}
