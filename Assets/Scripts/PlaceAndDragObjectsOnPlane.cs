using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Script to manipulate AR objects with translation, scaling, and rotation modes, supporting composite transformations.
/// For tutorial video, see my YouTube channel: <seealso href="https://www.youtube.com/@xiennastudio">YouTube channel</seealso>
/// How to use this script:
/// - Add ARPlaneManager to XROrigin GameObject.
/// - Add ARRaycastManager to XROrigin GameObject.
/// - Create Input Actions in Unity (TouchControls with <Pointer>/press and <Touchscreen>/touchCount).
/// - Attach this script to XROrigin GameObject.
/// - Add prefabs to be spawned to the <see cref="placedPrefabs"/> field in the Inspector.
/// - Assign a ModeSelector component to <see cref="modeSelector"/> in the Inspector.
/// - Ensure a UI Canvas with toggles, toggle layouts, dropdown, and delete/clear buttons is set up with the ModeSelector script.
/// - Assign an emission map texture to <see cref="emissionMap"/> for the outline effect.
///
/// Interaction:
/// - Enable modes via ModeSelector UI toggles (Translation, Scaling, Rotation), which can be active simultaneously.
/// - Select a prefab to spawn via the dropdown.
/// - Translation (if enabled):
///   - Click a spawned object to select it; a pulsing emission outline highlights it, and the delete button appears.
///   - Click another object to switch selection.
///   - Drag while selected to move it to the touch position on a plane.
///   - Release touch to stop dragging (object remains selected, outline persists).
///   - Click an empty plane space to deselect, hiding the delete button.
///   - Click a plane with no selected object to spawn the selected prefab.
/// - Scaling (if enabled):
///   - Two-finger pinch scales the selected object on enabled axes (X, Y, Z).
/// - Rotation (if enabled):
///   - Two-finger twist rotates the selected object around enabled axes (X, Y, Z).
/// - Delete button: Deletes the selected object and hides itself.
/// - Clear button: Deletes all spawned objects and resets selection.
/// Uses Unity's new Input System with Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch API and ARRaycastManager.
/// </summary>
[HelpURL("https://youtu.be/HkNVp04GOEI")]
[RequireComponent(typeof(ARRaycastManager))]
public class PlaceAndDragObjectsOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("List of prefabs that can be spawned, selected via dropdown.")]
    List<GameObject> placedPrefabs;

    [SerializeField]
    [Tooltip("Emission map texture to apply to the selected object's material.")]
    Texture2D emissionMap;

    [SerializeField]
    [Tooltip("Reference to the ModeSelector component that manages interaction modes and UI.")]
    ModeSelector modeSelector;

    [SerializeField]
    [Tooltip("Speed of the emission pulsing effect (cycles per second).")]
    float pulseSpeed = 1f;

    [SerializeField]
    [Tooltip("Sensitivity of the pinch scaling gesture (higher values scale faster).")]
    float scaleSensitivity = 0.01f;

    [SerializeField]
    [Tooltip("Minimum scale allowed for objects per axis.")]
    float minScale = 0.1f;

    [SerializeField]
    [Tooltip("Maximum scale allowed for objects per axis.")]
    float maxScale = 10f;

    [SerializeField]
    [Tooltip("Sensitivity of the twist rotation gesture (degrees per frame).")]
    float rotationSensitivity = 1f;

    [SerializeField]
    [Tooltip("Maximum distance from the object's position to consider it selected (in meters).")]
    float selectionDistanceThreshold = 0.1f;

    List<GameObject> spawnedObjects = new List<GameObject>();
    GameObject selectedItem;
    private bool isDragging;
    TouchControls controls;
    ARRaycastManager aRRaycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float previousPinchDistance;
    private float previousTwistAngle;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        if (aRRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager component not found!");
        }

        if (placedPrefabs == null || placedPrefabs.Count == 0 || placedPrefabs.Contains(null))
        {
            Debug.LogError("PlacedPrefabs list is empty or contains null entries in the Inspector!");
        }
        else
        {
            Debug.Log($"PlacedPrefabs assigned: {string.Join(", ", placedPrefabs.ConvertAll(p => p.name))}");
            if (modeSelector != null)
            {
                List<string> prefabNames = placedPrefabs.ConvertAll(p => p.name);
                modeSelector.PopulateDropdown(prefabNames);
            }
        }

        if (modeSelector == null)
        {
            Debug.LogError("ModeSelector component not assigned in the Inspector!");
        }
        else
        {
            modeSelector.OnDeleteButtonClicked += DeleteSelectedItem;
            modeSelector.OnClearButtonClicked += ClearAllItems;
        }

        if (pulseSpeed <= 0)
        {
            Debug.LogWarning("Pulse speed must be positive. Setting to default (1).");
            pulseSpeed = 1f;
        }

        if (scaleSensitivity <= 0)
        {
            Debug.LogWarning("Scale sensitivity must be positive. Setting to default (0.01).");
            scaleSensitivity = 0.01f;
        }

        if (minScale <= 0)
        {
            Debug.LogWarning("Minimum scale must be positive. Setting to default (0.1).");
            minScale = 0.1f;
        }

        if (maxScale < minScale)
        {
            Debug.LogWarning("Maximum scale must be greater than minimum scale. Setting to default (10).");
            maxScale = 10f;
        }

        if (rotationSensitivity <= 0)
        {
            Debug.LogWarning("Rotation sensitivity must be positive. Setting to default (1).");
            rotationSensitivity = 1f;
        }

        EnhancedTouchSupport.Enable();
        Debug.Log("Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch Support enabled.");

        controls = new TouchControls();
        controls.control.touch.started += ctx =>
        {
            if (ctx.control.device is Pointer device)
            {
                OnTouchStarted(device.position.ReadValue());
            }
        };
        controls.control.touch.canceled += _ => OnTouchCanceled();
    }

    private void OnEnable()
    {
        controls.control.Enable();
        Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch controls enabled.");
    }

    private void OnDisable()
    {
        controls.control.Disable();
        Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch controls disabled.");
        if (selectedItem != null)
        {
            DisableEmission(selectedItem);
        }
        if (modeSelector != null)
        {
            modeSelector.OnDeleteButtonClicked -= DeleteSelectedItem;
            modeSelector.OnClearButtonClicked -= ClearAllItems;
        }
        EnhancedTouchSupport.Disable();
        Debug.Log("Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch Support disabled.");
    }

    private void Update()
    {
        // Handle Translation (dragging)
        if (selectedItem != null && modeSelector.IsTranslationEnabled && isDragging)
        {
            Vector2 screenPosition = Pointer.current.position.ReadValue();
            if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                selectedItem.transform.position = hitPose.position;
                Vector3 lookPos = Camera.main.transform.position - selectedItem.transform.position;
                lookPos.y = 0;
                selectedItem.transform.rotation = Quaternion.LookRotation(lookPos);
                Debug.Log($"Dragging selected object to: {hitPose.position} (ScreenPos: {screenPosition})");
            }
            else
            {
                Debug.Log("Drag failed: UnityEngine.InputSystem.EnhancedTouch.Touch did not hit a plane.");
            }
        }

        // Handle Scaling (pinch)
        if (selectedItem != null && modeSelector.IsScalingEnabled && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
        {
            Vector2 touch0Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            Vector2 touch1Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
            float currentDistance = Vector2.Distance(touch0Pos, touch1Pos);

            if (previousPinchDistance > 0)
            {
                float scaleFactor = currentDistance / previousPinchDistance;
                Vector3 currentScale = selectedItem.transform.localScale;
                Vector3 newScale = currentScale;

                if (modeSelector.IsScaleXEnabled)
                    newScale.x = Mathf.Clamp(currentScale.x * scaleFactor, minScale, maxScale);
                if (modeSelector.IsScaleYEnabled)
                    newScale.y = Mathf.Clamp(currentScale.y * scaleFactor, minScale, maxScale);
                if (modeSelector.IsScaleZEnabled)
                    newScale.z = Mathf.Clamp(currentScale.z * scaleFactor, minScale, maxScale);

                if (newScale != currentScale)
                {
                    selectedItem.transform.localScale = newScale;
                    Debug.Log($"Scaling object: {selectedItem.name} to scale {newScale}");
                }
            }
            previousPinchDistance = currentDistance;
        }
        else
        {
            previousPinchDistance = 0;
        }

        // Handle Rotation (twist)
        if (selectedItem != null && modeSelector.IsRotationEnabled && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
        {
            Vector2 touch0Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            Vector2 touch1Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
            Vector2 delta = touch1Pos - touch0Pos;
            float currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (previousTwistAngle != 0)
            {
                float angleDelta = currentAngle - previousTwistAngle;
                Vector3 rotationDelta = Vector3.zero;

                if (modeSelector.IsRotateXEnabled)
                    rotationDelta.x = angleDelta * rotationSensitivity;
                if (modeSelector.IsRotateYEnabled)
                    rotationDelta.y = angleDelta * rotationSensitivity;
                if (modeSelector.IsRotateZEnabled)
                    rotationDelta.z = angleDelta * rotationSensitivity;

                if (rotationDelta != Vector3.zero)
                {
                    selectedItem.transform.Rotate(rotationDelta, Space.Self);
                    Debug.Log($"Rotating object: {selectedItem.name} by {rotationDelta}");
                }
            }
            previousTwistAngle = currentAngle;
        }
        else
        {
            previousTwistAngle = 0;
        }

        // Handle pulsing emission effect
        if (selectedItem != null)
        {
            Renderer[] renderers = selectedItem.transform.root.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                    Color emissionColor = Color.Lerp(new Color(0.588f, 0.588f, 0.588f), new Color(0.078f, 0.078f, 0.078f), t);
                    renderer.material.SetColor("_EmissionColor", emissionColor);
                }
            }
        }
    }

    void OnTouchStarted(Vector3 screenPosition)
    {
        // Check if the touch is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch is over a UI element, ignoring AR interaction.");
            return;
        }
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (IsTouchOverUI(touch.screenPosition))
            {
                Debug.Log("Touch is over a UI element, ignoring AR interaction.");
                return;
            }
        }

        // Handle spawning, selection, and deselection (always active)
        HandleTouch(screenPosition);
    }

    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        if (EventSystem.current == null)
            return false;

        // Create a PointerEventData with the touch position
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };

        // Raycast to check if the touch hits any UI elements
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Return true if any UI element was hit
        return results.Count > 0;
    }

    void HandleTouch(Vector3 screenPosition)
    {
        if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            // Check for object selection
            foreach (var obj in spawnedObjects)
            {
                if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
                {
                    if (selectedItem != obj)
                    {
                        if (selectedItem != null)
                        {
                            DisableEmission(selectedItem);
                            Debug.Log($"Switching selection from: {selectedItem.name} to: {obj.name}");
                        }
                        selectedItem = obj;
                        EnableEmission(selectedItem);
                        isDragging = modeSelector.IsTranslationEnabled; // Only drag if Translation is enabled
                        // modeSelector.SetDeleteButtonVisibility(true);
                        Debug.Log($"Selected object: {selectedItem.name}");
                    }
                    else if (modeSelector.IsTranslationEnabled)
                    {
                        isDragging = true;
                        Debug.Log($"Resumed dragging for: {selectedItem.name}");
                    }
                    return;
                }
            }

            // Check for deselection
            if (selectedItem != null)
            {
                bool isEmptySpace = true;
                foreach (var obj in spawnedObjects)
                {
                    if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
                    {
                        isEmptySpace = false;
                        break;
                    }
                }
                if (isEmptySpace)
                {
                    DisableEmission(selectedItem);
                    Debug.Log($"Deselected object: {selectedItem.name}");
                    selectedItem = null;
                    isDragging = false;
                    modeSelector.SetDeleteButtonVisibility(false);
                    return;
                }
            }

            // Spawn a new object
            if (selectedItem == null)
            {
                int prefabIndex = modeSelector.GetSelectedPrefabIndex();
                if (prefabIndex >= 0 && prefabIndex < placedPrefabs.Count && placedPrefabs[prefabIndex] != null)
                {
                    var newObject = Instantiate(placedPrefabs[prefabIndex], hitPose.position, hitPose.rotation);
                    spawnedObjects.Add(newObject);
                    Vector3 lookPos = Camera.main.transform.position - newObject.transform.position;
                    lookPos.y = 0;
                    newObject.transform.rotation = Quaternion.LookRotation(lookPos);
                    Debug.Log($"Spawned object: {newObject.name} at: {hitPose.position}");
                }
                else
                {
                    Debug.LogError($"Invalid prefab index: {prefabIndex} or null prefab.");
                }
            }
        }
        else
        {
            Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch did not hit a plane.");
        }
    }

    void EnableEmission(GameObject obj)
    {
        Renderer[] renderers = obj.transform.root.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Material material = renderer.material;
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (emissionMap != null)
                {
                    material.SetTexture("_EmissionMap", emissionMap);
                }
            }
        }
    }

    void DisableEmission(GameObject obj)
    {
        Renderer[] renderers = obj.transform.root.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Material material = renderer.material;
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                material.SetTexture("_EmissionMap", null);
            }
        }
    }

    void OnTouchCanceled()
    {
        if (isDragging)
        {
            isDragging = false;
            Debug.Log($"Stopped dragging, object remains selected: {(selectedItem != null ? selectedItem.name : "None")}");
        }
        else
        {
            Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch canceled, no dragging was active.");
        }
        previousPinchDistance = 0;
        previousTwistAngle = 0;
    }

    void DeleteSelectedItem()
    {
        if (selectedItem != null)
        {
            // Ensure the selected item is removed from the spawnedObjects list
            if (spawnedObjects.Contains(selectedItem))
            {
                spawnedObjects.Remove(selectedItem);
            }

            // Destroy the selected item
            Destroy(selectedItem.gameObject);
            Debug.Log($"Deleted object: {selectedItem.name}");

            // Clear the selected item reference
            selectedItem = null;

            // Hide the delete button
            modeSelector.SetDeleteButtonVisibility(false);
        }
        else
        {
            Debug.LogWarning("No selected item to delete.");
        }
    }

    void ClearAllItems()
    {
        foreach (var obj in spawnedObjects)
        {
            DisableEmission(obj);
            Destroy(obj);
        }
        spawnedObjects.Clear();
        if (selectedItem != null)
        {
            selectedItem = null;
            modeSelector.SetDeleteButtonVisibility(false);
        }
        Debug.Log("Cleared all spawned objects.");
    }
}