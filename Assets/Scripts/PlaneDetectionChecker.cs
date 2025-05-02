using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionChecker : MonoBehaviour
{
    private ARPlaneManager planeManager;

    private void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager component not found!");
        }
        else
        {
            Debug.Log("ARPlaneManager found and initialized.");
        }
    }

    private void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to planesChanged event.");
        }
    }

    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed from planesChanged event.");
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Log added planes
        if (args.added != null && args.added.Count > 0)
        {
            foreach (var plane in args.added)
            {
                Debug.Log($"Plane added: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log updated planes
        if (args.updated != null && args.updated.Count > 0)
        {
            foreach (var plane in args.updated)
            {
                Debug.Log($"Plane updated: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log removed planes
        if (args.removed != null && args.removed.Count > 0)
        {
            foreach (var plane in args.removed)
            {
                Debug.Log($"Plane removed: ID={plane.trackableId}");
            }
        }

        // Log total number of active planes
        int totalPlanes = planeManager.trackables.count;
        Debug.Log($"Total active planes: {totalPlanes}");
    }

    // Optional: Method to manually check current planes (can be called from another script or UI)
    public void LogCurrentPlanes()
    {
        if (planeManager == null)
        {
            Debug.LogWarning("Cannot log planes: ARPlaneManager is null.");
            return;
        }

        int totalPlanes = planeManager.trackables.count;
        if (totalPlanes == 0)
        {
            Debug.Log("No planes currently detected.");
            return;
        }

        foreach (var plane in planeManager.trackables)
        {
            Debug.Log($"Current plane: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
        }
        Debug.Log($"Total current planes: {totalPlanes}");
    }
}