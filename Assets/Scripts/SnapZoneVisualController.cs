using UnityEngine;
using Oculus.Interaction;

public class SnapZoneVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SnapInteractor snapInteractor;
    [SerializeField] private Grabbable grabbablePart;
    [SerializeField] private GameObject visualMarker;

    [Header("Settings")]
    [SerializeField] private bool showWhenGrabbed = true;
    [SerializeField] private bool hideWhenSnapped = true;

    private bool isSnapped = false;
    private bool isGrabbed = false;
    private bool isInitialized = false;

    void Start()
    {
        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }

        isInitialized = true;
    }

    void OnEnable()
    {
        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }

        isSnapped = false;
        isGrabbed = false;

        if (grabbablePart != null)
        {
            grabbablePart.WhenPointerEventRaised += OnGrabbableEvent;
        }

        if (snapInteractor != null)
        {
            snapInteractor.WhenStateChanged += OnSnapStateChanged;
        }
    }

    void OnDisable()
    {
        if (grabbablePart != null)
        {
            grabbablePart.WhenPointerEventRaised -= OnGrabbableEvent;
        }

        if (snapInteractor != null)
        {
            snapInteractor.WhenStateChanged -= OnSnapStateChanged;
        }

        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }
    }

    private void OnGrabbableEvent(PointerEvent pointerEvent)
    {
        if (!isInitialized) return;

        if (pointerEvent.Type == PointerEventType.Select)
        {
            isGrabbed = true;
            UpdateVisual();
            Debug.Log($"Part grabbed - Visual: {visualMarker.activeSelf}");
        }
        else if (pointerEvent.Type == PointerEventType.Unselect)
        {
            isGrabbed = false;
            UpdateVisual();
            Debug.Log($"Part released - Visual: {visualMarker.activeSelf}");
        }
    }

    private void OnSnapStateChanged(InteractorStateChangeArgs args)
    {
        if (!isInitialized) return;

        if (args.NewState == InteractorState.Select)
        {
            isSnapped = true;
            UpdateVisual();
            Debug.Log($"Part snapped - Visual hidden");
        }
        else if (args.PreviousState == InteractorState.Select)
        {
            isSnapped = false;
            UpdateVisual();
            Debug.Log($"Part unsnapped");
        }
    }

    private void UpdateVisual()
    {
        if (visualMarker == null || !isInitialized) return;

        bool shouldShow = false;

        if (showWhenGrabbed && isGrabbed && !isSnapped)
        {
            shouldShow = true;
        }

        if (hideWhenSnapped && isSnapped)
        {
            shouldShow = false;
        }

        visualMarker.SetActive(shouldShow);
    }

    public void ShowVisual()
    {
        if (visualMarker != null)
        {
            visualMarker.SetActive(true);
        }
    }

    public void HideVisual()
    {
        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }
    }
}
