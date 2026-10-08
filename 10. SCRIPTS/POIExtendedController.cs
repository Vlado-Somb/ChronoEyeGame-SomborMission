using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Core POI behavior: 
///   • Auto‐tags/nests under its AR anchor 
///   • Loads all configuration from POIDefinitionFleet (id, popup, narrative, spawn delay) 
///   • Handles world‐space popup instantiation 
///   • Flashes on interaction and triggers narrative via MissionManager
///   • Exposes activation callback (used by FleetManager/Timeline) 
/// </summary>
[RequireComponent(typeof(Collider))]
public class POIExtendedController : MonoBehaviour
{
    [Header("Data Definition")]
    [Tooltip("Drag the POI Definition asset that holds ID, popup, narrative, spawn delay, etc.")]
    public POIDefinitionFleet definition;

    [Header("Runtime Overrides")]
    [Tooltip("If true, this object will be auto-tagged as 'POI' and placed on POI layer")]
    public bool autoTagAsPOI = true;

    // --- Cached definition fields ---
    private string _poiID;
    private bool _showHighlight;
    private GameObject _highlightPrefab;
    private bool _useWorldPopup;
    private GameObject _worldPopupPrefab;
    private string _popupText;
    private float _popupDelay;
    private bool _triggerNarrative;
    private DialogAsset _dialogAsset;

    // --- Pre-instantiated instances ---
    private GameObject _highlightInstance;
    private GameObject _popupInstance;
    private TMP_Text _popupTextComponent;

    void Awake()
    {
        Debug.Log($"[POIExtended] {name}.Awake(): definition={(definition != null ? definition.id : "null")} ");
        // 1) Validate definition
        if (definition == null)
        {
            Debug.LogError($"[POIExtended] Missing POIDefinitionFleet on '{name}'");
            enabled = false;
            return;
        }

        if (string.IsNullOrEmpty(definition.id))
        {
            Debug.LogError($"[POIExtended] Definition.id is null or empty on '{name}'");
            enabled = false;
            return;
        }

        // Snapshot config
        _poiID = definition.id;
        _showHighlight = definition.showHighlight;
        _highlightPrefab = definition.highlightPrefab;
        _useWorldPopup = definition.useWorldPopup;
        _worldPopupPrefab = definition.worldPopupPrefab;
        _popupText = definition.popupText;
        _popupDelay = definition.popupDelay;
        _triggerNarrative = definition.triggerNarrative;
        _dialogAsset = definition.dialogAsset;

        // Auto-tag / layer
        if (autoTagAsPOI)
        {
            if (tag != "POI") tag = "POI";
            var poiLayer = LayerMask.NameToLayer("POI");
            if (gameObject.layer != poiLayer)
                gameObject.layer = poiLayer;
        }

        // Pre-instantiate highlight
        if (_showHighlight && _highlightPrefab != null)
        {
            _highlightInstance = Instantiate(_highlightPrefab, transform);
            _highlightInstance.SetActive(false);
        }

        // Pre-instantiate popup
        if (_useWorldPopup && _worldPopupPrefab != null)
        {
            _popupInstance = Instantiate(_worldPopupPrefab, transform);
            _popupInstance.transform.localPosition += Vector3.up * 1f;
            Debug.Log($"[POI] Instantiating popup at {transform.position}");
            _popupInstance.SetActive(false);

            if (_popupInstance.GetComponent<Billboard>() == null)
                _popupInstance.AddComponent<Billboard>();

            _popupTextComponent = _popupInstance.GetComponentInChildren<TMP_Text>();
            if (_popupTextComponent != null)
                _popupTextComponent.text = _popupText;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by FleetManager (or Timeline) when this POI should activate.
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);
        Debug.Log($"[POIExtended] Activate() called on '{name}' (ID = '{_poiID}')");
        TriggerSenderHelpers.FirePOITrigger(_poiID, TriggerSubEventType.POIActivated);

        if (_showHighlight && _highlightInstance != null)
        {
            _highlightInstance.SetActive(true);
            Debug.Log($"[POIExtended] Showing highlight for POI '{_poiID}'");
        }

        if (_useWorldPopup && _popupInstance != null)
        {
            Debug.Log($"[POIExtended] Scheduling world popup for POI '{_poiID}' after {_popupDelay}s");
            StartCoroutine(ShowPopupAfterDelay());
        }

        Debug.Assert(gameObject.activeInHierarchy, $"Cannot start popup coroutine: '{name}' is inactive!");
    }

    /* void LateUpdate()
    {
        if (Time.frameCount % 30 == 0) // every half second at 60fps
            Debug.Log($"Popup Rotation = {transform.rotation.eulerAngles}");
        Debug.Log($"Popup World Position: {transform.position}");
    } */

    private IEnumerator ShowPopupAfterDelay()
    {
        yield return new WaitForSeconds(_popupDelay);
        _popupInstance.SetActive(true);
        Debug.Log($"Popup instantiated at: {_popupInstance.transform.position}, localPosition: {_popupInstance.transform.localPosition}");

    }

    void OnDisable()
    {
        if (_highlightInstance != null)
            _highlightInstance.SetActive(false);
        if (_popupInstance != null)
            _popupInstance.SetActive(false);
    }

    /// <summary>
    /// Called by InteractionDetector when the user completes a hold-to-interact.
    /// Notifies MissionManager to handle narrative injection.
    /// </summary>
    [Obsolete]
    public void OnInteract()
    {
        StartCoroutine(FlashRed());
        Debug.Log($"[POIExtended] Interact fired on '{name}' (ID = '{_poiID}')");

        if (_triggerNarrative && _dialogAsset != null)
        {
            Debug.Log($"[POIExtended] OnInteract → MissionManager.OnPOIInteraction('{_poiID}')");
            TriggerSenderHelpers.FirePOITrigger(_poiID, TriggerSubEventType.POIInteracted);
            MissionManager.Instance.OnPOIInteraction(_poiID);
        }
        else if (_triggerNarrative)
        {
            Debug.LogWarning($"[POIExtended] definition.dialogAsset is null on POI '{_poiID}'. Cannot play any dialogue.");
        }
    }

    private IEnumerator FlashRed()
    {
        var rend = GetComponent<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        rend.material.color = original;
        yield return new WaitForSeconds(0.15f);
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        rend.material.color = original;
    }
}
