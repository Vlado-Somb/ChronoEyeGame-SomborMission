using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Simple tester that spawns a Cube prefab at the touch location without using ARAnchorManager.
/// Allows verifying that AR-placed objects receive tap+hold interactions via POIInteractionDetector.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class POIARAnchorCubeTester : MonoBehaviour
{
    [Header("Tester Prefab")]
    [Tooltip("Assign a CubePOI prefab (BoxCollider, POIExtendedController, POI layer)")]
    [SerializeField] private GameObject cubePrefab;

    private ARRaycastManager raycastManager;
    private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
#if UNITY_EDITOR
        // For editor testing with mouse
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = Input.mousePosition;
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 screenPos = Input.GetTouch(0).position;
#endif
            if (raycastManager.Raycast(screenPos, s_Hits, TrackableType.Planes | TrackableType.FeaturePoint))
            {
                Pose hitPose = s_Hits[0].pose;
                if (cubePrefab != null)
                {
                    var cube = Instantiate(cubePrefab, hitPose.position, hitPose.rotation);
                    Debug.Log($"[Tester] Placed cube at {hitPose.position}");
                }
            }
        }
    }
}
