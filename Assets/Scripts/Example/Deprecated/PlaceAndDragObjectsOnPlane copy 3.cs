// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// /// <summary>
// /// Script to manipulate AR objects with translation, scaling, and rotation modes.
// /// For tutorial video, see my YouTube channel: <seealso href="https://www.youtube.com/@xiennastudio">YouTube channel</seealso>
// /// How to use this script:
// /// - Add ARPlaneManager to XROrigin GameObject.
// /// - Add ARRaycastManager to XROrigin GameObject.
// /// - Create Input Actions in Unity (TouchControls with <Pointer>/press binding).
// /// - Attach this script to XROrigin GameObject.
// /// - Add the prefab to be spawned to the <see cref="placedPrefab"/> field in the Inspector.
// /// - Create a new input system called TouchControls with <Pointer>/press as the binding.
// /// - Assign a ModeSelector component to <see cref="modeSelector"/> in the Inspector.
// /// - Ensure a UI Canvas with three buttons (Translation, Scaling, Rotation) is set up with the ModeSelector script.
// ///
// /// Interaction:
// /// - Select a mode via the ModeSelector UI buttons (Translation, Scaling, Rotation).
// /// - In Translation mode:
// ///   - Click a spawned object to select it.
// ///   - Drag while selected to move it to the touch position on a plane (handled in Update).
// ///   - Release to deselect the object.
// ///   - Click a plane with no selected object to spawn a new object.
// /// - Scaling and Rotation modes: To be implemented.
// /// Uses Unity's new Input System and ARRaycastManager for object selection.
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
//     /// Reference to the ModeSelector component.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("The ModeSelector component that manages interaction modes.")]
//     ModeSelector modeSelector;

//     /// <summary>
//     /// List of all spawned objects.
//     /// </summary>
//     List<GameObject> spawnedObjects = new List<GameObject>();

//     /// <summary>
//     /// The currently selected object (if any).
//     /// </summary>
//     GameObject selectedItem;

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

//         if (modeSelector == null)
//         {
//             Debug.LogError("ModeSelector component not assigned in the Inspector!");
//         }

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
//     }

//     private void Update()
//     {
//         if (selectedItem != null && modeSelector.CurrentMode == ModeSelector.InteractionMode.Translation)
//         {
//             // Get the current pointer position
//             Vector2 screenPosition = Pointer.current.position.ReadValue();
//             // Move the selected object to the raycast position on the plane
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
//     }

//     void OnTouchStarted(Vector3 screenPosition)
//     {
//         // Handle touch based on the current mode
//         switch (modeSelector.CurrentMode)
//         {
//             case ModeSelector.InteractionMode.Translation:
//                 HandleTranslationTouch(screenPosition);
//                 break;
//             case ModeSelector.InteractionMode.Scaling:
//                 // To be implemented
//                 Debug.Log("Scaling mode not implemented yet.");
//                 break;
//             case ModeSelector.InteractionMode.Rotation:
//                 // To be implemented
//                 Debug.Log("Rotation mode not implemented yet.");
//                 break;
//         }
//     }

//     void HandleTranslationTouch(Vector3 screenPosition)
//     {
//         // Check if the touch hits a spawned object using ARRaycastManager
//         if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             var hitPose = hits[0].pose;
//             if (selectedItem == null)
//             {
//                 foreach (var obj in spawnedObjects)
//                 {
//                     if (Vector3.Distance(obj.transform.position, hitPose.position) < selectionDistanceThreshold)
//                     {
//                         selectedItem = obj;
//                         Debug.Log($"Selected object: {selectedItem.name}");
//                         return; // Exit to avoid spawning
//                     }
//                 }
//             }
//         }

//         // If no object is selected, spawn a new object on a plane
//         if (selectedItem == null && aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             var hitPose = hits[0].pose;
//             var newObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
//             spawnedObjects.Add(newObject);
//             Vector3 lookPos = Camera.main.transform.position - newObject.transform.position;
//             lookPos.y = 0;
//             newObject.transform.rotation = Quaternion.LookRotation(lookPos);
//             Debug.Log($"Spawned object at: {hitPose.position}");
//         }
//         else if (!aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             Debug.Log("Touch did not hit a plane.");
//         }
//     }

//     void OnTouchCanceled()
//     {
//         if (selectedItem != null)
//         {
//             Debug.Log($"Deselected object: {selectedItem.name}");
//             selectedItem = null;
//         }
//     }
// }