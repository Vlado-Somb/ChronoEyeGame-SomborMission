using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages activation of POIs in the scene. POIExtendedController components should be pre-placed under their AR anchors,
/// with their POIDefinitionFleet assigned in the Inspector. This manager exposes methods to activate individual or all POIs,
/// respecting each definition's spawnDelay, and fires an event when activation occurs.
/// Designed for MasterTimelineManager to call as needed.
/// </summary>
public class POIFleetManager : MonoBehaviour
{

    public static POIFleetManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[FleetManager] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// All POIExtendedController instances in the scene. Populate this list via Inspector or dynamically at Start().
    /// Each controller must have its POIDefinitionFleet assigned.
    /// </summary>
    [Tooltip("List of all POIExtendedController instances in the scene, with their definitions assigned.")]
    public List<POIExtendedController> poiControllers = new List<POIExtendedController>();

    /// <summary>
    /// Fired immediately after a POI is activated (SetActive and Activate() called).
    /// Provides the controller of the newly activated POI.
    /// </summary>
    public event Action<POIExtendedController> OnPOIActivated;

    /// <summary>
    /// Activate a single POI by its definition ID. Respects spawnDelay defined in POIDefinitionFleet.
    /// </summary>
    /// <param name="poiId">Unique identifier matching POIDefinitionFleet.id.</param>
    /// 
    public event Action<POIExtendedController> OnPOIActivationFailed;
    public event Action<POIExtendedController> OnPOIActivationStarted;
    // (i.e. the moment we begin activation, so we can subscribe to OnPOIConfirmed)

    public void ActivatePOIById(string poiId)
    {
        /* var ext = poiControllers.Find(e => e.definition != null && e.definition.id == poiId);
        if (ext == null)
        {
            Debug.LogWarning($"[FleetManager] No POI found with ID '{poiId}'. Activation failed.");
            OnPOIActivationFailed?.Invoke(null);
            return;
        }
        OnPOIActivationStarted?.Invoke(ext);*/

        var ext = poiControllers.Find (ctrl => ctrl.definition.id == poiId);
        if (ext == null)
        {
            Debug.Log($"[FleetManager] No POI found with ID '{poiId}'. Activation skipped.");
            OnPOIActivationStarted?.Invoke(null);
            return;
        }

        // 1) Enable first
        ext.gameObject.SetActive(true);
        Debug.Log($"[FleetManager] Activated POI '{poiId}' (GameObject '{ext.gameObject.name}').");

        // 2) Let the controller spawn its highlight/popup now that it's active
        ext.Activate();

        // 3) Let everyone know the POI is active
        OnPOIActivationStarted?.Invoke(ext);
        StartCoroutine(ActivatePOIAfterDelay(ext));
    }


    /// <summary>
    /// Activate all POIs in the list, each respecting its individual spawnDelay.
    /// </summary>
    public void ActivateAllPOIs()
    {
        foreach (var ext in poiControllers)
        {
            if (ext == null || ext.definition == null)
            {
                Debug.LogWarning("[FleetManager] Encountered null POIController or missing definition during ActivateAll.");
                continue;
            }
            StartCoroutine(ActivatePOIAfterDelay(ext));
        }
    }

    /// <summary>
    /// Coroutine: waits for the POI's defined spawnDelay, then sets it active and invokes activation logic.
    /// </summary>
    private IEnumerator ActivatePOIAfterDelay(POIExtendedController ext)
    {
        float delay = ext.definition != null ? ext.definition.spawnDelay : 0f;
        if (delay > 0f)
            yield return new WaitForSeconds(delay);


        // Activate the GameObject
        ext.gameObject.SetActive(true);
        Debug.Log($"[FleetManager] Activated POI GameObject'{ext.definition.id}' (GameObject '{ext.name}').");
        // Trigger the POI's own activation behavior (highlight, popup, etc.)
        ext.Activate();
        Debug.Log($"[FleetManager] Activated POI Extender");

        // Fire global event
        OnPOIActivated?.Invoke(ext);
    }
}
