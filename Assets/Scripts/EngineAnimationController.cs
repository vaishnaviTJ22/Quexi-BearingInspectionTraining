using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class EngineAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Name of the explode animation state")]
    public string explodeAnimationName = "BallBearing_Explode";

    [Tooltip("Name of the reassemble animation state")]
    public string reassembleAnimationName = "BallBearing_Reassemble";

    [Header("Events")]
    public UnityEvent onExplodeComplete;
    public UnityEvent onReassembleComplete;
    public UnityEvent<bool> onAllPartsSnappedChanged;

    [Header("Grabbable Parts")]
    [Tooltip("Add all parts that should be grabbable only when exploded")]
    public Grabbable[] grabbableParts;
    public GameObject[] grabbableGO;

    [Header("Snap Interactions")]
    [Tooltip("Add all SnapInteractors (children of grabbable parts)")]
    public SnapInteractor[] snapInteractors;

    [Tooltip("Add all Snap Zone parent GameObjects (e.g., Snap - Outer, Snap - Inner, etc.)")]
    public GameObject[] snapZones;

    [Header("Annotations")]
    [Tooltip("Add all Annotation GameObjects to enable in exploded view")]
    public GameObject[] annotations;

    [Header("Info UI")]
    [SerializeField] private PartInfoUIManager partInfoUIManager;

    [Header("Bearing Grabbable")]
    public Grabbable bearingGrabbable;
    public GameObject bearingGrabbableGO;

    [Header("Bearing SnapInteractor")]
    public SnapInteractor bearingSnapInteractor;
    public GameObject bearingSnapZone;

    public GameObject snapOutline;

    private Animator animator;
    private bool isExploded = false;
    private bool isAnimating = false;
    private bool isPlayingExplode = false;
    private bool allPartsSnapped = false;
    private bool isSubscribedToSnapEvents = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = false;
        }

        SetGrabbingEnabled(false);
        SetSnapZonesActive(false);
        SetAnnotationsActive(false);

        if (partInfoUIManager == null)
        {
            partInfoUIManager = FindFirstObjectByType<PartInfoUIManager>();
        }

        if (partInfoUIManager != null)
        {
            partInfoUIManager.ShowBearingOverview();
        }
        bearingSnapInteractor.WhenStateChanged += OnBearingSnapStateChanged;
    }
    private void OnBearingSnapStateChanged(InteractorStateChangeArgs args)
    {
        bool isSnapped = CheckBearingSnapState();

        EventManager.UpdateMenuUIActiveState?.Invoke(isSnapped);
    }
    bool CheckBearingSnapState()
    {
        if (bearingSnapInteractor != null && bearingSnapInteractor.enabled)
        {
            if (bearingSnapInteractor.State != InteractorState.Select)
            {
                return false;
            }
        }
        return true;
    }
    void OnDestroy()
    {
        UnsubscribeFromSnapEvents();
    }

    void Update()
    {
        if (isAnimating)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (isPlayingExplode && stateInfo.IsName(explodeAnimationName))
            {
                if (stateInfo.normalizedTime >= 0.99f)
                {
                    isAnimating = false;
                    isExploded = true;

                    animator.enabled = false;

                    SetGrabbingEnabled(true);
                    SetSnapZonesActive(true);
                    SetAnnotationsActive(true);
                    SubscribeToSnapEvents();

                    partInfoUIManager.HideBearingOverview();

                    onExplodeComplete?.Invoke();

                    Debug.Log("Explode animation complete - Animator disabled, parts grabbable, snap zones active, annotations visible");
                }
            }
            else if (!isPlayingExplode && stateInfo.IsName(reassembleAnimationName))
            {
                if (stateInfo.normalizedTime >= 0.99f)
                {
                    isAnimating = false;
                    isExploded = false;
                    allPartsSnapped = false;

                    animator.enabled = false;

                    partInfoUIManager.ShowBearingOverview();

                    onReassembleComplete?.Invoke();
                    onAllPartsSnappedChanged?.Invoke(false);

                    Debug.Log("Reassemble animation complete - Animator disabled, back to normal state, showing bearing overview");
                }
            }
        }

        if (isExploded && !isAnimating)
        {
            CheckAllPartsSnapped();
        }
    }

    public void ToggleExplodedView(bool explode)
    {
        if (isAnimating) return;

        if (explode && !isExploded)
        {
            PlayExplodeAnimation();
        }
        else if (!explode && isExploded)
        {
            if (AreAllPartsSnapped())
            {
                PlayReassembleAnimation();
            }
            else
            {
                Debug.LogWarning("Cannot reassemble - not all parts are snapped!");
            }
        }
    }

    private void PlayExplodeAnimation()
    {
        if (animator == null) return;

        isAnimating = true;
        isPlayingExplode = true;

        animator.enabled = true;
        animator.Play(explodeAnimationName, 0, 0f);

        Debug.Log("Playing explode animation - Animator enabled");
    }

    private void PlayReassembleAnimation()
    {
        if (animator == null) return;

        UnsubscribeFromSnapEvents();

        SetGrabbingEnabled(false);
        SetSnapZonesActive(false);
        SetAnnotationsActive(false);

        isAnimating = true;
        isPlayingExplode = false;

        animator.enabled = true;
        animator.Play(reassembleAnimationName, 0, 0f);

        Debug.Log("Playing reassemble animation - All parts snapped, returning to normal, annotations hidden");
    }

    private void SubscribeToSnapEvents()
    {
        if (isSubscribedToSnapEvents) return;

        foreach (var snapInteractor in snapInteractors)
        {
            if (snapInteractor != null)
            {
                snapInteractor.WhenStateChanged += OnSnapStateChanged;
            }
        }

        isSubscribedToSnapEvents = true;
        Debug.Log("Subscribed to snap events");
    }

    private void UnsubscribeFromSnapEvents()
    {
        if (!isSubscribedToSnapEvents) return;

        foreach (var snapInteractor in snapInteractors)
        {
            if (snapInteractor != null)
            {
                snapInteractor.WhenStateChanged -= OnSnapStateChanged;
            }
        }

        isSubscribedToSnapEvents = false;
        Debug.Log("Unsubscribed from snap events");
    }

    private void OnSnapStateChanged(InteractorStateChangeArgs args)
    {
        CheckAllPartsSnapped();
    }

    private void CheckAllPartsSnapped()
    {
        bool currentSnapState = AreAllPartsSnapped();

        if (currentSnapState != allPartsSnapped)
        {
            allPartsSnapped = currentSnapState;
            onAllPartsSnappedChanged?.Invoke(allPartsSnapped);

            Debug.Log($"All parts snapped: {allPartsSnapped}");
        }
    }

    public bool AreAllPartsSnapped()
    {
        if (!isExploded)
        {
            return false;
        }

        foreach (var snapInteractor in snapInteractors)
        {
            if (snapInteractor != null && snapInteractor.enabled)
            {
                if (snapInteractor.State != InteractorState.Select)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SetGrabbingEnabled(bool enabled)
    {
        foreach (var grabbable in grabbableParts)
        {
            if (grabbable != null)
            {
                grabbable.enabled = enabled;
            }
        }
        foreach (var grabbableGO in grabbableGO)
        {
            if (grabbableGO != null)
            {
                grabbableGO.SetActive(enabled);
            }
        }
        bearingGrabbable.enabled = !enabled;
        bearingGrabbableGO.SetActive(!enabled);
    }

    private void SetSnapZonesActive(bool active)
    {
        foreach (var snapInteractor in snapInteractors)
        {
            if (snapInteractor != null)
            {
                snapInteractor.enabled = active;
            }
        }

        foreach (var snapZone in snapZones)
        {
            if (snapZone != null)
            {
                snapZone.SetActive(active);
            }
        }

        bearingSnapInteractor.enabled = !active;
        bearingSnapZone.SetActive(!active);
    }

    private void SetAnnotationsActive(bool active)
    {
        foreach (var annotation in annotations)
        {
            if (annotation != null)
            {
                annotation.SetActive(active);
                Debug.Log($"Annotation {annotation.name} set to {active}");
            }
        }
    }

    public bool IsExploded()
    {
        return isExploded;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }
}
