// POIDefinitionFleet.cs
using UnityEngine;

/// <summary>
/// Data‐driven definition for a single POI in your AR fleet.
/// Designers can create multiple assets specifying prefab, ID,
/// highlight, popup, narrative settings, and spawn timing.
/// </summary>
[CreateAssetMenu(menuName = "AR/POI Definition", fileName = "New POI Definition")]
public class POIDefinitionFleet : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("Unique identifier for this POI (used at runtime)")]
    public string id;

    [Header("Prefab")]
    [Tooltip("Which POI prefab to instantiate or activate under its anchor")]
    public GameObject prefab;

    [Header("Highlight")]
    [Tooltip("Whether this POI should visually highlight when activated")]
    public bool showHighlight = true;

    [Tooltip("If you want to instantiate a separate highlight prefab (instead of toggling a child), assign it here.")]
    public GameObject highlightPrefab;

    [Header("UI Popup")]
    [Tooltip("Enable a world‐space text popup above the POI?")]
    public bool useWorldPopup = false;

    [Tooltip("Prefab containing a Canvas + Text (e.g. TextMeshPro) for world‐space popup.")]
    public GameObject worldPopupPrefab;

    [Tooltip("Text to display on the popup if enabled")]
    public string popupText;

    [Tooltip("Seconds to wait before showing the popup after spawn")]
    public float popupDelay = 0.5f;

    [Header("Narrative")]
    [Tooltip("Trigger a dialogue event on interaction?")]
    public bool triggerNarrative = false;

    [Tooltip("Connect DialogAsset (ScriptableObject) to play on interaction")]
    public DialogAsset dialogAsset;

    [Header("Spawn Timing")]
    [Tooltip("Delay in seconds after anchor readiness before activating this POI")]
    public float spawnDelay = 0f;
}
