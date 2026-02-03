using Lean.Localization;
using TamilUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExplodedViewUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EngineAnimationController animationController;
    [SerializeField] private PartInfoUIManager partInfoUIManager;
    [SerializeField] private Toggle explodedViewToggle;
    [SerializeField] private GameObject menuUIPanel;

    [Header("Button Label Settings")]
    [SerializeField] private TextMeshProUGUI buttonLabelText;
    [SerializeField] private string explodedViewLabel = "Exploded View";
    [SerializeField] private string normalViewLabel = "Normal View";
    [SerializeField] private string explodedViewPhrase;
    [SerializeField] private string explodedViewFallbackText;
    [SerializeField] private string normalViewPhrase;
    [SerializeField] private string normalViewFallbackText;

    void Start()
    {
        if (explodedViewToggle != null)
        {
            explodedViewToggle.isOn = false;
            explodedViewToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        if (partInfoUIManager == null)
        {
            partInfoUIManager = FindFirstObjectByType<PartInfoUIManager>();
        }

        if (partInfoUIManager != null && partInfoUIManager.CurrentSelectedBearing != null)
        {
            UpdateSelectedBearing(partInfoUIManager.CurrentSelectedBearing);
        }

        UpdateButtonLabel(false);
        UpdateToggleInteractable(true);
    }

    public void UpdateSelectedBearing(GameObject bearing)
    {
        if (bearing == null)
        {
            Debug.LogWarning("ExplodeViewUIController: Received null bearing!");
            return;
        }

        if (animationController != null)
        {
            animationController.onExplodeComplete.RemoveListener(OnExplodeComplete);
            animationController.onReassembleComplete.RemoveListener(OnReassembleComplete);
            animationController.onAllPartsSnappedChanged.RemoveListener(OnAllPartsSnappedChanged);
        }

        animationController = bearing.GetComponent<EngineAnimationController>();

        if (animationController == null)
        {
            Debug.LogError($"ExplodeViewUIController: Bearing '{bearing.name}' doesn't have EngineAnimationController component!");
            return;
        }

        animationController.onExplodeComplete.AddListener(OnExplodeComplete);
        animationController.onReassembleComplete.AddListener(OnReassembleComplete);
        animationController.onAllPartsSnappedChanged.AddListener(OnAllPartsSnappedChanged);

        explodedViewToggle.isOn = false;
        UpdateButtonLabel(false);
        UpdateToggleInteractable(true);

        Debug.Log($"ExplodeViewUIController: Switched to bearing '{bearing.name}'");
    }

    private void OnEnable()
    {
        EventManager.UpdateMenuUIActiveState += UpdateMenuActiveState;
        EventManager.UpdateSelectedBearing += UpdateSelectedBearing;
    }

    private void OnDisable()
    {
        EventManager.UpdateMenuUIActiveState -= UpdateMenuActiveState;
        EventManager.UpdateSelectedBearing -= UpdateSelectedBearing;
    }

    public void UpdateMenuActiveState(bool active)
    {
        menuUIPanel.SetActive(active);
    }

    void OnDestroy()
    {
        if (explodedViewToggle != null)
        {
            explodedViewToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        if (animationController != null)
        {
            animationController.onExplodeComplete.RemoveListener(OnExplodeComplete);
            animationController.onReassembleComplete.RemoveListener(OnReassembleComplete);
            animationController.onAllPartsSnappedChanged.RemoveListener(OnAllPartsSnappedChanged);
        }
    }

    [ContextMenu("ON")]
    public void On()
    {
        animationController.ToggleExplodedView(true);
        UpdateButtonLabel(true);
    }

    [ContextMenu("OFF")]
    public void Off()
    {
        if (animationController.AreAllPartsSnapped())
        {
            animationController.ToggleExplodedView(false);
            UpdateButtonLabel(false);
        }
        else
        {
            explodedViewToggle.isOn = true;
            Debug.LogWarning("Cannot reassemble - all parts must be snapped first!");
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (animationController != null && !animationController.IsAnimating())
        {
            if (isOn)
            {
                animationController.ToggleExplodedView(true);
                UpdateButtonLabel(true);
            }
            else
            {
                if (animationController.AreAllPartsSnapped())
                {
                    animationController.ToggleExplodedView(false);
                    UpdateButtonLabel(false);
                }
                else
                {
                    explodedViewToggle.isOn = true;
                    Debug.LogWarning("Cannot reassemble - all parts must be snapped first!");
                }
            }
        }
        else if (animationController != null && animationController.IsAnimating())
        {
            explodedViewToggle.isOn = !isOn;
        }
    }

    private void OnExplodeComplete()
    {
        UpdateToggleInteractable(false);
    }

    private void OnReassembleComplete()
    {
        UpdateToggleInteractable(true);
    }

    private void OnAllPartsSnappedChanged(bool allSnapped)
    {
        if (animationController.IsExploded())
        {
            UpdateToggleInteractable(allSnapped);
        }
    }

    private void UpdateButtonLabel(bool isExploded)
    {
        if (buttonLabelText != null)
        {
            var localizer = buttonLabelText.GetComponent<LeanLocalizedTamilTextMeshProUGUI>();

            if (localizer != null)
            {
                if (isExploded)
                {
                    localizer.TranslationName = normalViewPhrase;
                    localizer.FallbackText = normalViewFallbackText;
                }
                else
                {
                    localizer.TranslationName = explodedViewPhrase;
                    localizer.FallbackText = explodedViewFallbackText;
                }

                localizer.UpdateLocalization();
            }
            else
            {
                string currentPhrase = isExploded ? normalViewPhrase : explodedViewPhrase;
                string currentFallback = isExploded ? normalViewFallbackText : explodedViewFallbackText;
                buttonLabelText.text = GetLocalizedText(currentPhrase, currentFallback);
            }
        }
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

    private void UpdateToggleInteractable(bool interactable)
    {
        if (explodedViewToggle != null)
        {
            explodedViewToggle.interactable = interactable;
            Debug.Log($"Toggle button interactable: {interactable}");
        }
    }
}
