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
// /// - Add the prefab to be spawned to the <see cref="placedPrefab"/> field in the Inspector.
// /// - Assign a ModeSelector component to <see cref="modeSelector"/> in the Inspector.
// /// - Ensure a UI Canvas with three buttons (Translation, Scaling, Rotation) is set up with the ModeSelector script.
// /// - Create a transparent, emissive material for the outline effect and assign to <see cref="outlineMaterial"/>.
// ///
// /// Interaction:
// /// - Select a mode via the ModeSelector UI buttons (Translation, Scaling, Rotation).
// /// - In Translation mode:
// ///   - Click a spawned object to select it; a pulsing outline highlights it.
// ///   - If an object is selected, click another spawned object to switch selection.
// ///   - Drag while selected to move it to the touch position on a plane.
// ///   - Release touch to stop dragging (object remains selected, outline continues pulsing).
// ///   - Click an empty space on a plane to deselect the object, removing the outline.
// ///   - Click a plane with no selected object to spawn a new object.
// /// - In Scaling mode:
// ///   - Single-touch: Same as Translation mode for selection, deselection, spawning.
// ///   - Two-finger pinch: Scales the selected object up (pinch out) or down (pinch in) using Enhanced Touch API.
// /// - Rotation mode: To be implemented.
// /// Uses Unity's new Input System with Enhanced Touch API for pinch gestures and ARRaycastManager for object selection.
// /// </summary>
// [HelpURL("https://youtu.be/HkNVp04GOEI")]
// [RequireComponent(typeof(ARRaycastManager))]
// public class PlaceAndDragObjectsOnPlane : MonoBehaviour
// {
//     /// <summary>
//     /// The prefab that will be instantiated on touch.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Instantiates this prefab on a plane at the touch location.")]
//     GameObject placedPrefab;

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
//     [Tooltip("The ModeSelector component that manages interaction modes.")]
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
//     /// Minimum scale allowed for objects.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Minimum scale allowed for objects.")]
//     float minScale = 0.1f;

//     /// <summary>
//     /// Maximum scale allowed for objects.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Maximum scale allowed for objects.")]
//     float maxScale = 10f;

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

//     private void Awake()
//     {
//         aRRaycastManager = GetComponent<ARRaycastManager>();
//         if (aRRaycastManager == null)
//         {
//             Debug.LogError("ARRaycastManager component not found!");
//         }

//         if (placedPrefab == null)
//         {
//             Debug.LogError("PlacedPrefab is not assigned in the Inspector!");
//         }
//         else
//         {
//             Debug.Log($"PlacedPrefab assigned: {placedPrefab.name}");
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

//         // Enable Enhanced Touch Support
//         EnhancedTouchSupport.Enable();
//         Debug.Log("Enhanced Touch Support enabled.");

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
//         Debug.Log("Touch controls enabled.");
//     }

//     private void OnDisable()
//     {
//         controls.control.Disable();
//         Debug.Log("Touch controls disabled.");
//         // Clean up outline instance
//         if (outlineInstance != null)
//         {
//             Destroy(outlineInstance);
//         }
//         // Disable Enhanced Touch Support
//         EnhancedTouchSupport.Disable();
//         Debug.Log("Enhanced Touch Support disabled.");
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
//                 Debug.Log("Drag failed: Touch did not hit a plane.");
//             }
//         }

//         // Handle scaling in Scaling mode using Enhanced Touch API
//         if (selectedItem != null && modeSelector.CurrentMode == ModeSelector.InteractionMode.Scaling)
//         {
//             Debug.Log("Touch count: " + UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count);

//             if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
//             {
//                 Vector2 touch0Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
//                 Vector2 touch1Pos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
//                 float currentDistance = Vector2.Distance(touch0Pos, touch1Pos);

//                 // Check if this is the initial touch of the second finger
//                 if (previousPinchDistance == 0)
//                 {
//                     // Set the starting distance but do not scale yet
//                     previousPinchDistance = currentDistance;
//                     Debug.Log("Second finger detected, waiting for motion to start scaling.");
//                 }
//                 else
//                 {
//                     // Start scaling only after detecting motion
//                     float scaleFactor = currentDistance / previousPinchDistance;
//                     float currentScale = selectedItem.transform.localScale.x; // Assume uniform scale
//                     float newScale = Mathf.Clamp(currentScale * scaleFactor, minScale, maxScale);
//                     selectedItem.transform.localScale = Vector3.one * newScale;
//                     Debug.Log($"Scaling object: {selectedItem.name} to scale {newScale}");

//                     // Update the previous pinch distance
//                     previousPinchDistance = currentDistance;
//                 }
//             }
//             else
//             {
//                 // Reset when not pinching
//                 previousPinchDistance = 0;
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
//                 Debug.Log("Rotation mode not implemented yet.");
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
//                     if (selectedItem != obj) // Select a new object if different from current
//                     {
//                         if (selectedItem != null)
//                         {
//                             // Remove previous outline
//                             if (outlineInstance != null)
//                             {
//                                 Destroy(outlineInstance);
//                             }
//                             Debug.Log($"Switching selection from: {selectedItem.name} to: {obj.name}");
//                         }
//                         selectedItem = obj;
//                         // Add outline as child
//                         outlineInstance = Instantiate(placedPrefab, obj.transform);
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
//                         Debug.Log($"Selected object: {selectedItem.name}");
//                     }
//                     else // Resume dragging for the current selected object
//                     {
//                         isDragging = true;
//                         Debug.Log($"Resumed dragging for: {selectedItem.name}");
//                     }
//                     return; // Exit to avoid spawning or deselection
//                 }
//             }

//             // If an object is selected, check for deselection (click on empty plane space)
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
//                     // Remove outline and deselect
//                     if (outlineInstance != null)
//                     {
//                         Destroy(outlineInstance);
//                     }
//                     Debug.Log($"Deselected object: {selectedItem.name}");
//                     selectedItem = null;
//                     isDragging = false;
//                     return; // Exit to avoid spawning
//                 }
//             }

//             // If no object is selected, spawn a new object
//             if (selectedItem == null)
//             {
//                 var newObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
//                 spawnedObjects.Add(newObject);
//                 Vector3 lookPos = Camera.main.transform.position - newObject.transform.position;
//                 lookPos.y = 0;
//                 newObject.transform.rotation = Quaternion.LookRotation(lookPos);
//                 Debug.Log($"Spawned object at: {hitPose.position}");
//             }
//         }
//         else
//         {
//             Debug.Log("Touch did not hit a plane.");
//         }
//     }

//     void HandleScalingTouch(Vector3 screenPosition)
//     {
//         // Single-touch behavior mirrors Translation mode for selection/deselection/spawning
//         if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             // Only process single-touch if fewer than 2 touches
//             if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count <= 1)
//             {
//                 Debug.Log("Single touch detected in Scaling mode.");
//                 var hitPose = hits[0].pose;

//                 // Check for object selection (including switching to a different object)
//                 foreach (var obj in spawnedObjects)
//                 {
//                     if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
//                     {
//                         if (selectedItem != obj) // Select a new object if different from current
//                         {
//                             if (selectedItem != null)
//                             {
//                                 // Remove previous outline
//                                 if (outlineInstance != null)
//                                 {
//                                     Destroy(outlineInstance);
//                                 }
//                                 Debug.Log($"Switching selection from: {selectedItem.name} to: {obj.name}");
//                             }
//                             selectedItem = obj;
//                             // Add outline as child
//                             outlineInstance = Instantiate(placedPrefab, obj.transform);
//                             outlineInstance.transform.localScale = Vector3.one * outlineScale;
//                             outlineInstance.transform.localPosition = Vector3.zero;
//                             outlineInstance.transform.localRotation = Quaternion.identity;
//                             Renderer renderer = outlineInstance.GetComponent<Renderer>();
//                             if (renderer != null)
//                             {
//                                 renderer.material = outlineMaterial;
//                                 renderer.material.EnableKeyword("_EMISSION");
//                             }
//                             Debug.Log($"Selected object: {selectedItem.name}");
//                         }
//                         return; // Exit to avoid spawning or deselection
//                     }
//                 }

//                 // If an object is selected, check for deselection (click on empty plane space)
//                 if (selectedItem != null)
//                 {
//                     bool isEmptySpace = true;
//                     foreach (var obj in spawnedObjects)
//                     {
//                         if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
//                         {
//                             isEmptySpace = false;
//                             break;
//                         }
//                     }
//                     if (isEmptySpace)
//                     {
//                         // Remove outline and deselect
//                         if (outlineInstance != null)
//                         {
//                             Destroy(outlineInstance);
//                         }
//                         Debug.Log($"Deselected object: {selectedItem.name}");
//                         selectedItem = null;
//                         return; // Exit to avoid spawning
//                     }
//                 }

//                 // If no object is selected, spawn a new object
//                 if (selectedItem == null)
//                 {
//                     var newObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
//                     spawnedObjects.Add(newObject);
//                     Vector3 lookPos = Camera.main.transform.position - newObject.transform.position;
//                     lookPos.y = 0;
//                     newObject.transform.rotation = Quaternion.LookRotation(lookPos);
//                     Debug.Log($"Spawned object at: {hitPose.position}");
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("Touch did not hit a plane.");
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
//             Debug.Log("Touch canceled, no dragging was active.");
//         }
//         previousPinchDistance = 0; // Reset pinch distance
//     }
// }