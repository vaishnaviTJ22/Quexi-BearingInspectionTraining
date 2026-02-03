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
    [SerializeField] private bool autoUpdateWithSelectedBearing = true;

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

        if (autoUpdateWithSelectedBearing)
        {
            InitializeWithCurrentBearing();
        }
    }

    void OnEnable()
    {
        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }

        isSnapped = false;
        isGrabbed = false;

        SubscribeToCurrentReferences();

        if (autoUpdateWithSelectedBearing)
        {
            EventManager.UpdateSelectedBearing += OnBearingChanged;
        }
    }

    void OnDisable()
    {
        UnsubscribeFromCurrentReferences();

        if (autoUpdateWithSelectedBearing)
        {
            EventManager.UpdateSelectedBearing -= OnBearingChanged;
        }

        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }
    }

    private void InitializeWithCurrentBearing()
    {
        if (PartInfoUIManager.Instance != null && PartInfoUIManager.Instance.CurrentSelectedBearing != null)
        {
            UpdateBearingReferences(PartInfoUIManager.Instance.CurrentSelectedBearing);
        }
    }

    private void OnBearingChanged(GameObject newBearing)
    {
        if (newBearing == null)
        {
            Debug.LogWarning("SnapZoneVisualController: Received null bearing!");
            return;
        }

        UpdateBearingReferences(newBearing);
    }

    private void UpdateBearingReferences(GameObject bearingGameObject)
    {
        UnsubscribeFromCurrentReferences();

        snapInteractor = bearingGameObject.GetComponentInChildren<SnapInteractor>();
        grabbablePart = bearingGameObject.GetComponent<Grabbable>();

        visualMarker = bearingGameObject.GetComponent<EngineAnimationController>().snapOutline;

        if (snapInteractor == null)
        {
            Debug.LogError($"SnapZoneVisualController: Bearing '{bearingGameObject.name}' doesn't have SnapInteractor component!");
        }

        if (grabbablePart == null)
        {
            Debug.LogError($"SnapZoneVisualController: Bearing '{bearingGameObject.name}' doesn't have Grabbable component!");
        }

        isSnapped = false;
        isGrabbed = false;

        SubscribeToCurrentReferences();

        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }

        Debug.Log($"SnapZoneVisualController: Updated to bearing '{bearingGameObject.name}'");
    }

    private void SubscribeToCurrentReferences()
    {
        if (grabbablePart != null)
        {
            grabbablePart.WhenPointerEventRaised += OnGrabbableEvent;
        }

        if (snapInteractor != null)
        {
            snapInteractor.WhenStateChanged += OnSnapStateChanged;
        }
    }

    private void UnsubscribeFromCurrentReferences()
    {
        if (grabbablePart != null)
        {
            grabbablePart.WhenPointerEventRaised -= OnGrabbableEvent;
        }

        if (snapInteractor != null)
        {
            snapInteractor.WhenStateChanged -= OnSnapStateChanged;
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

    public void SetBearingReferences(SnapInteractor newSnapInteractor, Grabbable newGrabbablePart)
    {
        UnsubscribeFromCurrentReferences();

        snapInteractor = newSnapInteractor;
        grabbablePart = newGrabbablePart;

        isSnapped = false;
        isGrabbed = false;

        SubscribeToCurrentReferences();

        if (visualMarker != null)
        {
            visualMarker.SetActive(false);
        }

        Debug.Log($"SnapZoneVisualController: Manually updated references");
    }
}
