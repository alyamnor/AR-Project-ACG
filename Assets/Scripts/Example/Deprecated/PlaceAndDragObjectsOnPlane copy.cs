// using System.Collections.Generic;
// using System.ComponentModel;
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
// /// - Ensure the prefab has a Collider component for selection detection.
// /// - Create a new input system called TouchControls with <Pointer>/press as the binding.
// ///
// /// Touch to select a spawned object, move the selected object to a new plane position, spawn a new object, or deselect.
// /// - Click a spawned object to select it.
// /// - Click a plane while an object is selected to move the object to that position.
// /// - Click a plane with no selected object to spawn a new object.
// /// - Click elsewhere (not on an object or plane) with a selected object to deselect it.
// /// Uses Unity's new Input System.
// /// </summary>
// [HelpURL("https://youtu.be/HkNVp04GOEI")]
// [RequireComponent(typeof(ARRaycastManager))]
// public class PlaceAndDragObjectsOnPlane : MonoBehaviour
// {
//     /// <summary>
//     /// The prefab that will be instantiated on touch.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Instantiates this prefab on a plane at the touch location. Must have a Collider for selection.")]
//     GameObject placedPrefab;

//     /// <summary>
//     /// The currently spawned object (if any).
//     /// </summary>
//     GameObject spawnedObject;

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
//         // Handle touch start events
//         controls.control.touch.started += ctx =>
//         {
//             if (ctx.control.device is Pointer device)
//             {
//                 OnTouchStarted(device.position.ReadValue());
//             }
//         };
//         controls.control.touch.performed += ctx =>
//         {
//             if (ctx.control.device is Pointer device)
//             {
//                 OnTouchPerformed(device.position.ReadValue());
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

//     // void OnTouchStarted(Vector3 screenPosition)
//     // {
//     //     // Check if the touch hits the spawned object using ARRaycastManager
//     //     if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//     //     {
//     //         var hitPose = hits[0].pose;
//     //         if (spawnedObject != null && Vector3.Distance(spawnedObject.transform.position, hitPose.position) < 0.1f && selectedItem == null)
//     //         {
//     //             // Select the spawned object if it's touched
//     //             selectedItem = spawnedObject;
//     //             Debug.Log($"Selected object: {selectedItem.name}");
//     //             return; // Exit to avoid other actions
//     //         }
//     //     }

//     //     // Check if the touch hits a plane
//     //     if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//     //     {
//     //         var hitPose = hits[0].pose;

//     //         if (selectedItem != null)
//     //         {
//     //             // Move the selected object to the new position
//     //             selectedItem.transform.position = hitPose.position;
//     //             // Update rotation to face the camera
//     //             Vector3 lookPos = Camera.main.transform.position - selectedItem.transform.position;
//     //             lookPos.y = 0;
//     //             selectedItem.transform.rotation = Quaternion.LookRotation(lookPos);
//     //             Debug.Log($"Moved selected object to: {hitPose.position}");
//     //         }
//     //         else
//     //         {
//     //             // Spawn a new object only if no object is selected
//     //             if (spawnedObject == null)
//     //             {
//     //                 spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
//     //                 Vector3 lookPos = Camera.main.transform.position - spawnedObject.transform.position;
//     //                 lookPos.y = 0;
//     //                 spawnedObject.transform.rotation = Quaternion.LookRotation(lookPos);
//     //                 Debug.Log($"Spawned object at: {hitPose.position}");
//     //             }
//     //         }
//     //     }
//     //     else
//     //     {
//     //         // If no plane or object was hit and an item is selected, deselect it
//     //         if (selectedItem != null)
//     //         {
//     //             Debug.Log($"Deselected object: {selectedItem.name}");
//     //             selectedItem = null;
//     //         }
//     //         else
//     //         {
//     //             Debug.Log("Touch did not hit a plane or object.");
//     //         }
//     //     }
//     // }

//     void OnTouchStarted(Vector3 screenPosition)
//     {
//         // Check if the touch hits the spawned object using ARRaycastManager
//         if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             var hitPose = hits[0].pose;
//             if (spawnedObject != null && selectedItem == null &&
//                 Vector3.Distance(spawnedObject.transform.position, hitPose.position) < selectionDistanceThreshold)
//             {
//                 // Select the spawned object if it's touched
//                 selectedItem = spawnedObject;
//                 Debug.Log($"Selected object: {selectedItem.name}");
//                 return; // Exit to avoid spawning
//             }
//         }

//         // If no object is selected, try to spawn a new object on a plane
//         if (selectedItem == null && aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             var hitPose = hits[0].pose;
//             if (spawnedObject == null)
//             {
//                 spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
//                 Vector3 lookPos = Camera.main.transform.position - spawnedObject.transform.position;
//                 lookPos.y = 0;
//                 spawnedObject.transform.rotation = Quaternion.LookRotation(lookPos);
//                 Debug.Log($"Spawned object at: {hitPose.position}");
//             }
//             else
//             {
//                 Debug.Log("Cannot spawn: An object already exists.");
//             }
//         }
//         else if (!aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//         {
//             Debug.Log("Touch did not hit a plane.");
//         }
//     }

//     void OnTouchPerformed(Vector3 screenPosition)
//     {
//         if (selectedItem != null)
//         {
//             // Move the selected object to the touch position on the plane
//             if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
//             {
//                 var hitPose = hits[0].pose;
//                 selectedItem.transform.position = hitPose.position;
//                 Vector3 lookPos = Camera.main.transform.position - selectedItem.transform.position;
//                 lookPos.y = 0;
//                 selectedItem.transform.rotation = Quaternion.LookRotation(lookPos);
//                 Debug.Log($"Dragging selected object to: {hitPose.position}");
//             }
//             else
//             {
//                 Debug.Log("Drag failed: Touch did not hit a plane.");
//             }
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