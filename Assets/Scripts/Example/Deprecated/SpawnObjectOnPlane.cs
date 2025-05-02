using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class SpawnObjectOnPlane : MonoBehaviour
{
    private ARRaycastManager aRRaycastManager;
    private GameObject spawnedObject;

    [SerializeField]
    private GameObject PlaceablePrefab;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        if (aRRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager component not found!");
        }
        if (PlaceablePrefab == null)
        {
            Debug.LogError("PlaceablePrefab is not assigned in the Inspector!");
        }
        else
        {
            Debug.Log($"PlaceablePrefab assigned: {PlaceablePrefab.name}");
        }
    }

    void OnEnable()
    {
        if (!UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.enabled)
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            Debug.Log("Enhanced Touch Support enabled.");
        }
    }

    void OnDisable()
    {
        if (UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.enabled)
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
            Debug.Log("Enhanced Touch Support disabled.");
        }
    }

    void Update()
    {
        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count == 0)
        {
            Debug.Log("No active touches detected.");
            return;
        }

        Debug.Log($"Detected {activeTouches.Count} touches at position: {activeTouches[0].screenPosition}");
        if (aRRaycastManager.Raycast(activeTouches[0].screenPosition, hits, TrackableType.Planes))
        {
            Debug.Log($"Raycast hit {hits.Count} planes at position: {hits[0].pose.position}");
            var hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(PlaceablePrefab, hitPose.position, hitPose.rotation);
                Debug.Log($"Spawned object at: {hitPose.position}");
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
                spawnedObject.transform.rotation = hitPose.rotation;
                Debug.Log($"Moved object to: {hitPose.position}");
            }
        }
        else
        {
            Debug.Log("Raycast failed to hit any planes!");
        }
    }
}