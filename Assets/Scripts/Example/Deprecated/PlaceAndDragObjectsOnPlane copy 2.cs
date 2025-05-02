// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// /// <summary>
// /// For tutorial video, see my YouTube channel: <seealso href="https://www.youtube.com/@xiennastudio">YouTube channel</seealso>
// /// How to use this script:
// /// - Add ARPlaneManager to XROrigin GameObject.
// /// - Add ARRaycastManager to XROrigin GameObject.
// /// - Create Input Actions in Unity (TouchControls with <Pointer>/press binding).
// /// - Attach this script to XROrigin GameObject.
// /// - Add the prefab to be spawned to the <see cref="placedPrefab"/> field in the Inspector.
// /// - Create a new input system called TouchControls with <Pointer>/press as the binding.
// ///
// /// Touch to select a spawned object, drag to move it on a plane, spawn multiple objects, or deselect.
// /// - Click a spawned object to select it.
// /// - Drag while an object is selected to move it to the touch position on a plane (handled in Update).
// /// - Release to deselect the object.
// /// - Click a plane with no selected object to spawn a new object.
// /// Uses Unity's new Input System and ARRaycastManager for object selection.
// /// </summary>
// [HelpURL("https://youtu.be/HkNVp04GOEI")]
// [RequireComponent(typeof(ARRaycastManager))]
// public class SelectAndDragMultipleObjectsOnPlane : MonoBehaviour
// {
//     /// <summary>
//     /// The prefab that will be instantiated on touch.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Instantiates this prefab on a plane at the touch location.")]
//     GameObject placedPrefab;

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

//         controls = new TouchControls();
//         // Handle touch input phases
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
//         if (selectedItem != null)
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