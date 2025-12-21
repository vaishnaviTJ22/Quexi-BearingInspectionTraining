using UnityEngine;

public class SmoothBillboardUI : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool autoFindCamera = true;
    [SerializeField] private bool reverseDirection = true;
    [SerializeField] private bool lockY = false;

    [Header("Smooth Rotation")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float snapThreshold = 1f;

    void Start()
    {
        if (autoFindCamera && cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning("SmoothBillboardUI: No camera found.");
            }
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 directionToCamera;

        if (reverseDirection)
        {
            directionToCamera = transform.position - cameraTransform.position;
        }
        else
        {
            directionToCamera = cameraTransform.position - transform.position;
        }

        if (lockY)
        {
            directionToCamera.y = 0;
        }

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

            float angle = Quaternion.Angle(transform.rotation, targetRotation);

            if (angle < snapThreshold)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
