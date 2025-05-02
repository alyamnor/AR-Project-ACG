using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Reference to the camera to control.")]
    private Camera targetCamera;

    [SerializeField]
    [Tooltip("Offset to apply to the camera's position.")]
    private Vector3 positionOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("Field of view for the camera (only applies to perspective projection).")]
    private float fieldOfView = 60f;

    [SerializeField]
    [Tooltip("Projection type for the camera (Perspective or Orthographic).")]
    private bool useOrthographic = false;

    [SerializeField]
    [Tooltip("Size of the orthographic camera view (only applies to orthographic projection).")]
    private float orthographicSize = 5f;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("No camera assigned or found in the scene.");
            return;
        }

        // Apply initial settings
        UpdateCameraProjection();
    }

    private void LateUpdate()
    {
        if (targetCamera != null)
        {
            // Apply position offset
            targetCamera.transform.position = transform.position + positionOffset;

            // Optionally, you can also control rotation here if needed
            // targetCamera.transform.rotation = Quaternion.Euler(0, 0, 0); // Example: Reset rotation
        }
    }

    private void UpdateCameraProjection()
    {
        if (targetCamera != null)
        {
            if (useOrthographic)
            {
                targetCamera.orthographic = true;
                targetCamera.orthographicSize = orthographicSize;
            }
            else
            {
                targetCamera.orthographic = false;
                targetCamera.fieldOfView = fieldOfView;
            }
            Debug.Log("targetCamera.orthographic: " + targetCamera.orthographic);
        }
    }

    // Optional: Public method to toggle projection type at runtime
    public void ToggleProjection()
    {
        useOrthographic = !useOrthographic;
        UpdateCameraProjection();
    }
}