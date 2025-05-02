// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.EnhancedTouch;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// /// <summary>
// /// Script to manipulate AR objects with translation, scaling, and rotation modes.
// /// For tutorial video, see my YouTube channel: <seealso href="https://www.youtube.com/@xiennastudio">YouTube channel</seealso>
// /// How to use this script:
// /// - Add ARPlaneManager to XROrigin GameObject.
// /// - Add ARRaycastManager to XROrigin GameObject.
// /// - Create Input Actions in Unity (TouchControls with <Pointer>/press and <Touchscreen>/touchCount).
// /// - Attach this script to XROrigin GameObject.
// /// - Add prefabs to be spawned to the <see cref="placedPrefabs"/> field in the Inspector.
// /// - Assign a ModeSelector component to <see cref="modeSelector"/> in the Inspector.
// /// - Ensure a UI Canvas with buttons, toggle layouts, dropdown, and delete/clear buttons is set up with the ModeSelector script.
// /// - Create a transparent, emissive material for the outline effect and assign to <see cref="outlineMaterial"/>.
// ///
// /// Interaction:
// /// - Select a mode via ModeSelector UI buttons (Translation, Scaling, Rotation).
// /// - Select a prefab to spawn via the dropdown.
// /// - In Translation mode:
// ///   - Click a spawned object to select it; a pulsing outline highlights it, and the delete button appears.
// ///   - If an object is selected, click another to switch selection.
// ///   - Drag while selected to move it to the touch position on a plane.
// ///   - Release touch to stop dragging (object remains selected, outline persists).
// ///   - Click an empty plane space to deselect, hiding the delete button.
// ///   - Click a plane with no selected object to spawn the selected prefab.
// /// - In Scaling mode:
// ///   - Single-touch: Same as Translation mode.
// ///   - Two-finger pinch: Scales the selected object on enabled axes (X, Y, Z).
// /// - In Rotation mode:
// ///   - Single-touch: Same as Translation mode.
// ///   - Two-finger twist: Rotates the selected object around enabled axes (X, Y, Z).
// /// - Delete button: Deletes the selected object and hides itself.
// /// - Clear button: Deletes all spawned objects and resets selection.
// /// Uses Unity's new Input System with Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch API and ARRaycastManager.
// /// </summary>
// [HelpURL("https://youtu.be/HkNVp04GOEI")]
// [RequireComponent(typeof(ARRaycastManager))]
// public class PlaceAndDragObjectsOnPlane : MonoBehaviour
// {
//     /// <summary>
//     /// List of prefabs that can be instantiated.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("List of prefabs that can be spawned, selected via dropdown.")]
//     List<GameObject> placedPrefabs;

//     /// <summary>
//     /// Material for the outline effect (transparent, emissive).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Material for the outline effect (use Standard shader, Transparent mode, emissive).")]
//     Material outlineMaterial;

//     /// <summary>
//     /// Reference to the ModeSelector component.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("The ModeSelector component that manages interaction modes and UI.")]
//     ModeSelector modeSelector;

//     /// <summary>
//     /// Speed of the pulsing outline effect (cycles per second).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Speed of the outline pulsing effect (cycles per second).")]
//     float pulseSpeed = 1f;

//     /// <summary>
//     /// Scale multiplier for the outline mesh (e.g., 1.1 for 10% larger).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Scale multiplier for the outline mesh relative to the object (e.g., 1.1 for 10% larger).")]
//     float outlineScale = 1.1f;

//     /// <summary>
//     /// Sensitivity of the pinch scaling gesture.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Sensitivity of the pinch scaling gesture (higher values scale faster).")]
//     float scaleSensitivity = 0.01f;

//     /// <summary>
//     /// Minimum scale allowed for objects per axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Minimum scale allowed for objects per axis.")]
//     float minScale = 0.1f;

//     /// <summary>
//     /// Maximum scale allowed for objects per axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Maximum scale allowed for objects per axis.")]
//     float maxScale = 10f;

//     /// <summary>
//     /// Sensitivity of the twist rotation gesture (degrees per frame).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Sensitivity of the twist rotation gesture (degrees per frame).")]
//     float rotationSensitivity = 1f;

//     /// <summary>
//     /// List of all spawned objects.
//     /// </summary>
//     List<GameObject> spawnedObjects = new List<GameObject>();

//     /// <summary>
//     /// The currently selected object (if any).
//     /// </summary>
//     GameObject selectedItem;

//     /// <summary>
//     /// Tracks whether the selected object is being dragged.
//     /// </summary>
//     private bool isDragging;

//     /// <summary>
//     /// The input touch control.
//     /// </summary>
//     TouchControls controls;

//     /// <summary>
//     /// Reference to the ARRaycastManager.
//     /// </summary>
//     ARRaycastManager aRRaycastManager;

//     /// <summary>
//     /// List to store raycast hits.
//     /// </summary>
//     List<ARRaycastHit> hits = new List<ARRaycastHit>();

//     /// <summary>
//     /// Distance threshold for detecting object selection (in meters).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Maximum distance from the object's position to consider it selected (in meters).")]
//     float selectionDistanceThreshold = 0.1f;

//     /// <summary>
//     /// The outline GameObject for the selected item.
//     /// </summary>
//     private GameObject outlineInstance;

//     /// <summary>
//     /// Previous distance between two touches for scaling.
//     /// </summary>
//     private float previousPinchDistance;

//     /// <summary>
//     /// Previous angle between two touches for rotation.
//     /// </summary>
//     private float previousTwistAngle;

//     private void Awake()
//     {
//         aRRaycastManager = GetComponent<ARRaycastManager>();
//         if (aRRaycastManager == null)
//         {
//             Debug.LogError("ARRaycastManager component not found!");
//         }

//         if (placedPrefabs == null || placedPrefabs.Count == 0 || placedPrefabs.Contains(null))
//         {
//             Debug.LogError("PlacedPrefabs list is empty or contains null entries in the Inspector!");
//         }
//         else
//         {
//             Debug.Log($"PlacedPrefabs assigned: {string.Join(", ", placedPrefabs.ConvertAll(p => p.name))}");
//             // Populate dropdown with prefab names
//             if (modeSelector != null)
//             {
//                 List<string> prefabNames = placedPrefabs.ConvertAll(p => p.name);
//                 modeSelector.PopulateDropdown(prefabNames);
//             }
//         }

//         if (outlineMaterial == null)
//         {
//             Debug.LogError("OutlineMaterial is not assigned in the Inspector!");
//         }
//         else
//         {
//             Debug.Log($"OutlineMaterial assigned: {outlineMaterial.name}");
//         }

//         if (modeSelector == null)
//         {
//             Debug.LogError("ModeSelector component not assigned in the Inspector!");
//         }
//         else
//         {
//             // Subscribe to delete and clear button events
//             modeSelector.OnDeleteButtonClicked += DeleteSelectedItem;
//             modeSelector.OnClearButtonClicked += ClearAllItems;
//         }

//         if (pulseSpeed <= 0)
//         {
//             Debug.LogWarning("Pulse speed must be positive. Setting to default (1).");
//             pulseSpeed = 1f;
//         }

//         if (outlineScale <= 1f)
//         {
//             Debug.LogWarning("Outline scale must be greater than 1. Setting to default (1.1).");
//             outlineScale = 1.1f;
//         }

//         if (scaleSensitivity <= 0)
//         {
//             Debug.LogWarning("Scale sensitivity must be positive. Setting to default (0.01).");
//             scaleSensitivity = 0.01f;
//         }

//         if (minScale <= 0)
//         {
//             Debug.LogWarning("Minimum scale must be positive. Setting to default (0.1).");
//             minScale = 0.1f;
//         }

//         if (maxScale < minScale)
//         {
//             Debug.LogWarning("Maximum scale must be greater than minimum scale. Setting to default (10).");
//             maxScale = 10f;
//         }

//         if (rotationSensitivity <= 0)
//         {
//             Debug.LogWarning("Rotation sensitivity must be positive. Setting to default (1).");
//             rotationSensitivity = 1f;
//         }

//         // Enable Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch Support
//         EnhancedTouchSupport.Enable();
//         Debug.Log("Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch Support enabled.");

//         controls = new TouchControls();
//         controls.control.touch.started += ctx =>
//         {
//             if (ctx.control.device is Pointer device)
//             {
//                 OnTouchStarted(device.position.ReadValue());
//             }
//         };
//         controls.control.touch.canceled += _ => OnTouchCanceled();
//     }

//     private void OnEnable()
//     {
//         controls.control.Enable();
//         Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch controls enabled.");
//     }

//     private void OnDisable()
//     {
//         controls.control.Disable();
//         Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch controls disabled.");
//         if (outlineInstance != null)
//         {
//             Destroy(outlineInstance);
//         }
//         if (modeSelector != null)
//         {
//             modeSelector.OnDeleteButtonClicked -= DeleteSelectedItem;
//             modeSelector.OnClearButtonClicked -= ClearAllItems;
//         }
//         EnhancedTouchSupport.Disable();
//         Debug.Log("Enhanced UnityEngine.InputSystem.EnhancedTouch.Touch Support disabled.");
//     }

//     private void Update()
//     {
//         // Handle dragging in Translation mode
//         if (selectedItem != null && modeSelector.CurrentMode == ModeSelector.InteractionMode.Translation && isDragging)
//         {
//             Vector2 screenPosition = Pointer.current.position.ReadValue();
//             if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//             {
//                 var hitPose = hits[0].pose;
//                 selectedItem.transform.position = hitPose.position;
//                 Vector3 lookPos = Camera.main.transform.position - selectedItem.transform.position;
//                 lookPos.y = 0;
//                 selectedItem.transform.rotation = Quaternion.LookRotation(lookPos);
//                 Debug.Log($"Dragging selected object to: {hitPose.position} (ScreenPos: {screenPosition})");
//             }
//             else
//             {
//                 Debug.Log("Drag failed: UnityEngine.InputSystem.EnhancedTouch.Touch did not hit a plane.");
//             }
//         }

//         // Handle scaling in Scaling mode
//         if (selectedItem != null && modeSelector.CurrentMode == ModeSelector.InteractionMode.Scaling)
//         {
//             if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
//             {
//                 Vector2 touch0Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
//                 Vector2 touch1Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
//                 float currentDistance = Vector2.Distance(touch0Pos, touch1Pos);

//                 if (previousPinchDistance > 0)
//                 {
//                     float scaleFactor = currentDistance / previousPinchDistance;
//                     Vector3 currentScale = selectedItem.transform.localScale;
//                     Vector3 newScale = currentScale;

//                     if (modeSelector.IsScaleXEnabled)
//                         newScale.x = Mathf.Clamp(currentScale.x * scaleFactor, minScale, maxScale);
//                     if (modeSelector.IsScaleYEnabled)
//                         newScale.y = Mathf.Clamp(currentScale.y * scaleFactor, minScale, maxScale);
//                     if (modeSelector.IsScaleZEnabled)
//                         newScale.z = Mathf.Clamp(currentScale.z * scaleFactor, minScale, maxScale);

//                     if (newScale != currentScale)
//                     {
//                         selectedItem.transform.localScale = newScale;
//                         Debug.Log($"Scaling object: {selectedItem.name} to scale {newScale}");
//                     }
//                 }
//                 previousPinchDistance = currentDistance;
//             }
//             else
//             {
//                 previousPinchDistance = 0;
//             }
//         }

//         // Handle rotation in Rotation mode
//         if (selectedItem != null && modeSelector.CurrentMode == ModeSelector.InteractionMode.Rotation)
//         {
//             if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
//             {
//                 Vector2 touch0Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
//                 Vector2 touch1Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
//                 Vector2 delta = touch1Pos - touch0Pos;
//                 float currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

//                 if (previousTwistAngle != 0)
//                 {
//                     float angleDelta = currentAngle - previousTwistAngle;
//                     Vector3 rotationDelta = Vector3.zero;

//                     if (modeSelector.IsRotateXEnabled)
//                         rotationDelta.x = angleDelta * rotationSensitivity;
//                     if (modeSelector.IsRotateYEnabled)
//                         rotationDelta.y = angleDelta * rotationSensitivity;
//                     if (modeSelector.IsRotateZEnabled)
//                         rotationDelta.z = angleDelta * rotationSensitivity;

//                     if (rotationDelta != Vector3.zero)
//                     {
//                         selectedItem.transform.Rotate(rotationDelta, Space.Self);
//                         Debug.Log($"Rotating object: {selectedItem.name} by {rotationDelta}");
//                     }
//                 }
//                 previousTwistAngle = currentAngle;
//             }
//             else
//             {
//                 previousTwistAngle = 0;
//             }
//         }

//         // Handle pulsing outline effect
//         if (outlineInstance != null)
//         {
//             Renderer renderer = outlineInstance.GetComponent<Renderer>();
//             if (renderer != null)
//             {
//                 float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
//                 Color emissionColor = Color.Lerp(Color.black, new Color(0.392f, 0.392f, 0.392f), t);
//                 renderer.material.SetColor("_EmissionColor", emissionColor);
//             }
//         }
//     }

//     void OnTouchStarted(Vector3 screenPosition)
//     {
//         switch (modeSelector.CurrentMode)
//         {
//             case ModeSelector.InteractionMode.Translation:
//                 HandleTranslationTouch(screenPosition);
//                 break;
//             case ModeSelector.InteractionMode.Scaling:
//                 HandleScalingTouch(screenPosition);
//                 break;
//             case ModeSelector.InteractionMode.Rotation:
//                 HandleRotationTouch(screenPosition);
//                 break;
//         }
//     }

//     void HandleTranslationTouch(Vector3 screenPosition)
//     {
//         if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             var hitPose = hits[0].pose;

//             // Check for object selection (including switching to a different object)
//             foreach (var obj in spawnedObjects)
//             {
//                 if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
//                 {
//                     if (selectedItem != obj)
//                     {
//                         if (selectedItem != null)
//                         {
//                             if (outlineInstance != null)
//                             {
//                                 Destroy(outlineInstance);
//                             }
//                             Debug.Log($"Switching selection from: {selectedItem.name} to: {obj.name}");
//                         }
//                         selectedItem = obj;
//                         int prefabIndex = modeSelector.GetSelectedPrefabIndex();
//                         outlineInstance = Instantiate(placedPrefabs[prefabIndex], obj.transform);
//                         outlineInstance.transform.localScale = Vector3.one * outlineScale;
//                         outlineInstance.transform.localPosition = Vector3.zero;
//                         outlineInstance.transform.localRotation = Quaternion.identity;
//                         Renderer renderer = outlineInstance.GetComponent<Renderer>();
//                         if (renderer != null)
//                         {
//                             renderer.material = outlineMaterial;
//                             renderer.material.EnableKeyword("_EMISSION");
//                         }
//                         isDragging = true;
//                         modeSelector.SetDeleteButtonVisibility(true);
//                         Debug.Log($"Selected object: {selectedItem.name}");
//                     }
//                     else
//                     {
//                         isDragging = true;
//                         Debug.Log($"Resumed dragging for: {selectedItem.name}");
//                     }
//                     return;
//                 }
//             }

//             // Check for deselection
//             if (selectedItem != null)
//             {
//                 bool isEmptySpace = true;
//                 foreach (var obj in spawnedObjects)
//                 {
//                     if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
//                     {
//                         isEmptySpace = false;
//                         break;
//                     }
//                 }
//                 if (isEmptySpace)
//                 {
//                     if (outlineInstance != null)
//                     {
//                         Destroy(outlineInstance);
//                     }
//                     Debug.Log($"Deselected object: {selectedItem.name}");
//                     selectedItem = null;
//                     isDragging = false;
//                     modeSelector.SetDeleteButtonVisibility(false);
//                     return;
//                 }
//             }

//             // Spawn a new object
//             if (selectedItem == null)
//             {
//                 int prefabIndex = modeSelector.GetSelectedPrefabIndex();
//                 if (prefabIndex >= 0 && prefabIndex < placedPrefabs.Count && placedPrefabs[prefabIndex] != null)
//                 {
//                     var newObject = Instantiate(placedPrefabs[prefabIndex], hitPose.position, hitPose.rotation);
//                     spawnedObjects.Add(newObject);
//                     Vector3 lookPos = Camera.main.transform.position - newObject.transform.position;
//                     lookPos.y = 0;
//                     newObject.transform.rotation = Quaternion.LookRotation(lookPos);
//                     Debug.Log($"Spawned object: {newObject.name} at: {hitPose.position}");
//                 }
//                 else
//                 {
//                     Debug.LogError($"Invalid prefab index: {prefabIndex} or null prefab.");
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch did not hit a plane.");
//         }
//     }

//     void HandleScalingTouch(Vector3 screenPosition)
//     {
//         if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count <= 1)
//         {
//             HandleTranslationTouch(screenPosition);
//         }
//     }

//     void HandleRotationTouch(Vector3 screenPosition)
//     {
//         if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count <= 1)
//         {
//             HandleTranslationTouch(screenPosition);
//         }
//     }

//     void OnTouchCanceled()
//     {
//         if (isDragging)
//         {
//             isDragging = false;
//             Debug.Log($"Stopped dragging, object remains selected: {(selectedItem != null ? selectedItem.name : "None")}");
//         }
//         else
//         {
//             Debug.Log("UnityEngine.InputSystem.EnhancedTouch.Touch canceled, no dragging was active.");
//         }
//         previousPinchDistance = 0;
//         previousTwistAngle = 0;
//     }

//     void DeleteSelectedItem()
//     {
//         if (selectedItem != null)
//         {
//             spawnedObjects.Remove(selectedItem);
//             if (outlineInstance != null)
//             {
//                 Destroy(outlineInstance);
//             }
//             Debug.Log($"Deleted object: {selectedItem.name}");
//             Destroy(selectedItem);
//             selectedItem = null;
//             modeSelector.SetDeleteButtonVisibility(false);
//         }
//     }

//     void ClearAllItems()
//     {
//         foreach (var obj in spawnedObjects)
//         {
//             Destroy(obj);
//         }
//         spawnedObjects.Clear();
//         if (selectedItem != null)
//         {
//             if (outlineInstance != null)
//             {
//                 Destroy(outlineInstance);
//             }
//             selectedItem = null;
//             modeSelector.SetDeleteButtonVisibility(false);
//         }
//         Debug.Log("Cleared all spawned objects.");
//     }
// }