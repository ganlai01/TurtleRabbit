using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetecter : MonoBehaviour
{
    private ARPlaneManager planeManager;
    // public turtleController turtleController;
    [SerializeField]
    private float accumulatedPlaneSize = 0.0f;
    private ARRaycastManager raycastManager; // Reference to ARRaycastManager
    private Dictionary<TrackableId, Vector2> planeSizeLookup = new Dictionary<TrackableId, Vector2>();
    private List<Vector3> spawnedItemsPosition = new List<Vector3>();
    private float distanceSpawnedItems = 0.25f;

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
        raycastManager = FindObjectOfType<ARRaycastManager>();
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

    #region OnPlanesChanged Event Handler
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Log added planes
        if (args.added != null && args.added.Count > 0)
        {
            foreach (var plane in args.added)
            {
                var size = plane.size;
                float area = size.x * size.y;

                planeSizeLookup[plane.trackableId] = size;
                accumulatedPlaneSize += area;

                Debug.Log($"Plane added: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log updated planes
        if (args.updated != null && args.updated.Count > 0)
        {
            foreach (var plane in args.updated)
            {
                var id = plane.trackableId;
                var newSize = plane.size;
                float newArea = newSize.x * newSize.y;

                if (planeSizeLookup.TryGetValue(id, out var oldSize))
                {
                    float oldArea = oldSize.x * oldSize.y;
                    accumulatedPlaneSize -= oldArea;
                }

                accumulatedPlaneSize += newArea;
                planeSizeLookup[id] = newSize;

                Debug.Log($"Plane updated: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log removed planes
        if (args.removed != null && args.removed.Count > 0)
        {
            foreach (var plane in args.removed)
            {
                var id = plane.trackableId;

                if (planeSizeLookup.TryGetValue(id, out var oldSize))
                {
                    float oldArea = oldSize.x * oldSize.y;
                    accumulatedPlaneSize -= oldArea;
                    planeSizeLookup.Remove(id);
                }

                Debug.Log($"Plane removed: ID={plane.trackableId}");
            }
        }
    }
    #endregion

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

    // #region SpawnTurtle
    // public void SpawnTurtle(Vector2 screenPosition)
    // {
    //     if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
    //     {
    //         Pose hitPose = raycastHits[0].pose;
    //         GameObject turtleObject = Instantiate(turtlePrefab, hitPose.position, Quaternion.identity);
    //         Debug.Log("turtle spawned at: " + hitPose.position);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No AR plane hit at tapped position.");
    //     }
    
    // }
    // #endregion
}