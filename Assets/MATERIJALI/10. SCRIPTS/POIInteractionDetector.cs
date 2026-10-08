
using System.Collections;
using UnityEngine;

/// <summary>
/// Detects tap-and-hold interactions on POI objects in AR.
/// Handles raycasting against the POI layer, showing a hold-to-interact UI panel,
/// and invoking the POI's interaction callback—but ONLY after a full holdDuration.
/// </summary>
[RequireComponent(typeof(Camera))]
public class POIInteractionDetector : MonoBehaviour
{
    [Header("Hold Settings")]
    [Tooltip("Seconds the player must hold to confirm interaction.")]
    public float holdDuration = 1f;

    [Header("Interaction Settings")]
    [Tooltip("Maximum distance (m) to raycast for POI hits.")]
    public float interactionDistance = 5f;

    [Header("UI Overlay")]
    [Tooltip("UI panel RectTransform for hold-to-interact feedback.")]
    public RectTransform holdPanel;
    [Tooltip("Vertical offset (m) above the POI for UI placement.")]
    public float yOffset = 1.5f;

    // Internal state for the hold logic
    private Coroutine _holdCoroutine;
    private POIExtendedController _currentExt;
    private Camera _cam;

    /// <summary>Fired when a POI has been confirmed via hold-to-interact.</summary>
    public event System.Action<POIExtendedController> OnPOIConfirmed;

    void Awake()
    {
        // Use the main camera for raycasts
        _cam = Camera.main;

        // Ensure the holdPanel is hidden initially
        if (holdPanel != null)
            holdPanel.gameObject.SetActive(false);
    }

    void Update()
    {

#if UNITY_EDITOR
        // In Editor: treat mouse left button like a single-finger touch
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[POIInteractionDetector] Editor: MouseButtonDown → TryStartHold");
            TryStartHold(Input.mousePosition);
        }
        if (_holdCoroutine != null && Input.GetMouseButtonUp(0))
        {
            Debug.Log("[POIInteractionDetector] Editor: MouseButtonUp → CancelHold");
            CancelHold();
        }
#else
        // On device: use touch input
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("[POIInteractionDetector] Touch Began → TryStartHold");
                TryStartHold(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (_holdCoroutine != null)
                {
                    Debug.Log("[POIInteractionDetector] Touch Ended/Canceled → CancelHold");
                    CancelHold();
                }
            }
        }
#endif

    }

    /// <summary>
    /// Called on pointer (mouse or touch) down. Performs a raycast at screenPos,
    /// and, if it hits a POIExtendedController within interactionDistance, starts the hold coroutine.
    /// </summary>
    private void TryStartHold(Vector2 screenPos)
    {
        // Raycast from the camera into the scene
        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, LayerMask.GetMask("POI")))
        {
            // Did we hit a POIExtendedController?
            if (hit.collider.TryGetComponent<POIExtendedController>(out var ext))
            {
                _currentExt = ext;
                Debug.Log($"[POIInteractionDetector] Raycast hit POI '{ext.definition.id}'. Starting hold panel.");
                ShowHoldPanel(ext.transform.position, screenPos);
                return;
            }
        }

        // If we get here, either we missed entirely or hit something that isn't a POI
        Debug.Log("[POIInteractionDetector] Raycast did not hit a valid POI → CancelHold");
        CancelHold();
    }

    /// <summary>
    /// Positions & shows the holdPanel above the POI, then starts the HoldTimer coroutine.
    /// </summary>
    private void ShowHoldPanel(Vector3 worldPos, Vector2 screenPos)
    {
        if (holdPanel == null || _currentExt == null)
            return;

        // Compute a screen position for the UI panel slightly above the POI
        Vector3 panelWorld = worldPos + Vector3.up * yOffset;
        Vector3 panelScreen = _cam.WorldToScreenPoint(panelWorld);
        holdPanel.position = panelScreen;
        holdPanel.gameObject.SetActive(true);

        // Launch the coroutine that waits holdDuration before confirming
        _holdCoroutine = StartCoroutine(HoldTimer(_currentExt));
    }

    /// <summary>
    /// Counts up from 0 to holdDuration, then invokes the POI's OnInteract and OnPOIConfirmed.
    /// If CancelHold is called first, this coroutine is stopped.
    /// </summary>
    [System.Obsolete]
    private IEnumerator HoldTimer(POIExtendedController ext)
    {
        float timer = 0f;
        Debug.Log($"[POIInteractionDetector] HoldTimer started for POI '{ext.definition.id}', duration = {holdDuration}s");

        while (timer < holdDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Once the full holdDuration has elapsed, fire the interaction
        Debug.Log($"[POIInteractionDetector] Hold complete → Invoking OnInteract() on '{ext.definition.id}'");
        ext.OnInteract();

        Debug.Log($"[POIInteractionDetector] Firing OnPOIConfirmed for '{ext.definition.id}'");
        OnPOIConfirmed?.Invoke(ext);

        // Clean up UI and reset state
        CancelHold();
    }

    /// <summary>
    /// Stops the hold coroutine (if running) and hides the holdPanel.
    /// Called on pointer up or if the user drags off-target.
    /// </summary>
    private void CancelHold()
    {
        if (_holdCoroutine != null)
        {
            Debug.Log("[POIInteractionDetector] CancelHold → Stopping HoldTimer coroutine.");
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }

        if (holdPanel != null)
            holdPanel.gameObject.SetActive(false);

        _currentExt = null;
    }
}
