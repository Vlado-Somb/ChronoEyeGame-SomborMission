using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks and registers POIs in a single component:
/// 1. Registers POIs when they are activated by the Fleet Manager.
/// 2. Deregisters POIs when they are deactivated or destroyed.
/// 3. Monitors registered POIs each frame for viewport and occlusion,
///    firing enter/exit events as POIs cross the camera view boundary.
/// </summary>
public class POIVisibilityTrackerRegistry : MonoBehaviour
{
    /// <summary>Invoked when a POI becomes active in the scene.</summary>
    public event Action<POIExtendedController> POIRegistered;

    /// <summary>Invoked when a POI is deactivated or removed.</summary>
    public event Action<POIExtendedController> POIDeregistered;

    /// <summary>Invoked when a registered POI first enters the camera viewport.</summary>
    public event Action<POIExtendedController> POIEnteredView;

    /// <summary>Invoked when a registered POI exits the camera viewport.</summary>
    public event Action<POIExtendedController> POIExitedView;

    private Camera _mainCamera;

    // All currently registered (active) POI controllers
    private readonly List<POIExtendedController> _registeredPOIs = new List<POIExtendedController>();

    // Track which POIs were visible last frame
    private readonly HashSet<POIExtendedController> _previouslyVisible = new HashSet<POIExtendedController>();

    // Track which POIs are visible this frame
    private readonly HashSet<POIExtendedController> _currentlyVisible = new HashSet<POIExtendedController>();

    private void OnEnable()
    {
        // Subscribe to the FleetManager's activation event
        if (POIFleetManager.Instance != null)
        {
            POIFleetManager.Instance.OnPOIActivated += HandlePOIActivation;
            Debug.Log("[POITrackerRegistry] Subscribed to POIFleetManager.OnPOIActivated");
        }
        else
        {
            Debug.LogWarning("[POITrackerRegistry] POIFleetManager.Instance is null on Enable");
        }
    }

    private void OnDisable()
    {
        if (POIFleetManager.Instance != null)
        {
            POIFleetManager.Instance.OnPOIActivated -= HandlePOIActivation;
            Debug.Log("[POITrackerRegistry] Unsubscribed from POIFleetManager.OnPOIActivated");
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("[POITrackerRegistry] MainCamera not found. Disabling component.");
            enabled = false;
            return;
        }
        Debug.Log($"[POITrackerRegistry] MainCamera found: '{_mainCamera.name}'. Starting up.");
    }

    /// <summary>
    /// Called when a POI is activated by the Fleet Manager.
    /// Registers the POI for visibility checks.
    /// </summary>
    private void HandlePOIActivation(POIExtendedController poi)
    {
        if (poi == null)
        {
            Debug.LogWarning("[POITrackerRegistry] Received null POI in HandlePOIActivation, ignoring.");
            return;
        }

        if (_registeredPOIs.Contains(poi))
        {
            Debug.Log($"[POITrackerRegistry] POI '{poi.name}' already registered, skipping.");
            return;
        }

        _registeredPOIs.Add(poi);
        Debug.Log($"[POITrackerRegistry] Registered POI: '{poi.name}' (Total: {_registeredPOIs.Count})");
        POIRegistered?.Invoke(poi);
    }

    private void Update()
    {
        // Remove any POIs that have become inactive or were destroyed
        for (int i = _registeredPOIs.Count - 1; i >= 0; i--)
        {
            var poi = _registeredPOIs[i];
            if (poi == null || !poi.gameObject.activeInHierarchy)
            {
                string poiName = poi != null ? poi.name : "<Destroyed>";
                _registeredPOIs.RemoveAt(i);
                Debug.Log($"[POITrackerRegistry] Deregistered POI: '{poiName}' (Remaining: {_registeredPOIs.Count})");
                POIDeregistered?.Invoke(poi);

                // Also clear any visibility tracking
                _previouslyVisible.Remove(poi);
                _currentlyVisible.Remove(poi);
            }
        }

        _currentlyVisible.Clear();

        // Check viewport and occlusion for each registered POI
        foreach (var poi in _registeredPOIs)
        {
            if (poi == null)
                continue;

            Transform t = poi.transform;
            Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(t.position);
            bool isInViewport = viewportPoint.z > 0f && viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f;

            if (isInViewport && IsLineOfSightClear(t))
            {
                _currentlyVisible.Add(poi);
            }
        }

        // Fire enter events for newly visible POIs
        foreach (var poi in _currentlyVisible)
        {
            if (!_previouslyVisible.Contains(poi))
            {
                Debug.Log($"[POITrackerRegistry] POI Entered View: '{poi.name}'");
                POIEnteredView?.Invoke(poi);
                TriggerSenderHelpers.FirePOITrigger(poi.definition.id, TriggerSubEventType.POIVisible);
            }
        }

        // Fire exit events for POIs no longer visible
        foreach (var poi in _previouslyVisible)
        {
            if (!_currentlyVisible.Contains(poi))
            {
                Debug.Log($"[POITrackerRegistry] POI Exited View: '{poi.name}'");
                POIExitedView?.Invoke(poi);
            }
        }

        // Prepare for next frame
        _previouslyVisible.Clear();
        foreach (var poi in _currentlyVisible)
        {
            _previouslyVisible.Add(poi);
        }
    }

    /// <summary>
    /// Returns true if no other collider blocks line-of-sight from the camera to the POI.
    /// </summary>
    private bool IsLineOfSightClear(Transform t)
    {
        Vector3 screenPoint = _mainCamera.WorldToScreenPoint(t.position);
        Ray ray = _mainCamera.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            bool clear = (hit.transform == t);
            if (!clear)
            {
                Debug.Log($"[POITrackerRegistry] Line of sight blocked for POI '{t.name}' by '{hit.transform.name}'");
            }
            return clear;
        }

        // No hit => nothing is blocking
        return true;
    }

    /// <summary>
    /// Provides a read-only list of all currently registered POIs.
    /// </summary>
    public IReadOnlyList<POIExtendedController> GetAllRegisteredPOIs()
    {
        return _registeredPOIs;
    }
}
