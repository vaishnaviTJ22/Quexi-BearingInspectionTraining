using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExplodedViewUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EngineAnimationController animationController;
    [SerializeField] private Toggle explodedViewToggle;

    [Header("Button Label Settings")]
    [SerializeField] private TextMeshProUGUI buttonLabelText;
    [SerializeField] private string explodedViewLabel = "Exploded View";
    [SerializeField] private string normalViewLabel = "Normal View";

    void Start()
    {
        if (explodedViewToggle != null)
        {
            explodedViewToggle.isOn = false;
            explodedViewToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        if (animationController != null)
        {
            animationController.onExplodeComplete.AddListener(OnExplodeComplete);
            animationController.onReassembleComplete.AddListener(OnReassembleComplete);
            animationController.onAllPartsSnappedChanged.AddListener(OnAllPartsSnappedChanged);
        }

        UpdateButtonLabel(false);
        UpdateToggleInteractable(true);
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
            buttonLabelText.text = isExploded ? normalViewLabel : explodedViewLabel;
        }
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
