using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Events;

public class GrabbableEnginePart : MonoBehaviour
{
    [Header("Part Configuration")]
    public EnginePartData partData;

    [Header("Events")]
    public UnityEvent<EnginePartData> onPartGrabbed;
    public UnityEvent onPartReleased;

    private Grabbable grabbable;
    private SnapInteractable snapInteractable;

    void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        snapInteractable = GetComponent<SnapInteractable>();
    }

    void Start()
    {
        if (partData != null && partData.explanationAudio != null)
        {
            if (PartAudioManager.Instance != null)
            {
                PartAudioManager.Instance.RegisterPartAudio(partData, partData.explanationAudio);
            }
        }
    }

    void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }
    }

    void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            onPartGrabbed?.Invoke(partData);

            if (PartAudioManager.Instance != null && partData != null)
            {
                PartAudioManager.Instance.OnPartGrabbed(partData);
            }

            Debug.Log($"Grabbed: {partData?.partName}");
        }
        else if (pointerEvent.Type == PointerEventType.Unselect)
        {
            onPartReleased?.Invoke();

            if (PartAudioManager.Instance != null && partData != null)
            {
                PartAudioManager.Instance.OnPartReleased(partData);
            }

            Debug.Log($"Released: {partData?.partName}");
        }
    }

    public void EnableGrabbing(bool enable)
    {
        if (grabbable != null)
        {
            grabbable.enabled = enable;
        }
        if (snapInteractable != null)
        {
            snapInteractable.enabled = enable;
        }
    }
}
