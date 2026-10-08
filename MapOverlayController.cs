using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapOverlayController : MonoBehaviour
{
    [Header("UI Toolkit References")]
    public UIDocument mapUIDocument;
    public Texture2D mapTexture;
    [SerializeField] private VisualTreeAsset waypointTemplateUXML;
    private WaypointSpawner waypointSpawner;

    [Header("Custom Icons")]
    [SerializeField] private Texture2D playerIconTexture;
    [SerializeField] private Texture2D waypointIconTexture;

    [SerializeField] WaypointDatabase waypointDatabase; // your existing ref

    [SerializeField] private WaypointManager waypointManager;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private MissionGPSHUDUIController gpsUI;
    [SerializeField] private HintUIController hintUIController;


    [Header("Map Image Resolution (px)")]
    public float mapImageWidth = 3189f;
    public float mapImageHeight = 3264f;


    private VisualElement mapRoot;
    private bool mapVisible = true;

    [Header("GPS Provider")]
    public UnifiedGpsProvider gpsProvider;

    [Header("GPS Bounds")]
    public double latTop = 45.77485478900039;
    public double latBottom = 45.76875802397785;
    public double lonLeft = 19.110463120985997;
    public double lonRight = 19.118941688845965;

    private VisualElement mapImage;
    private VisualElement playerMarker;
    private VisualElement waypointContainer;

    bool _bootstrapped;
    bool _waypointsSpawned;
    bool _panelReady;
    private bool _mapReady;

    private void Awake()
    {
        var root = mapUIDocument.rootVisualElement;
        mapRoot = root.Q<VisualElement>("MapRoot");
        mapImage = root.Q<VisualElement>("MapImage");
        playerMarker = root.Q<VisualElement>("PlayerMarker");
        waypointContainer = root.Q<VisualElement>("WaypointContainer");

        mapVisible = false;
        mapRoot.style.display = DisplayStyle.None;
        mapRoot.pickingMode = PickingMode.Ignore;
        _mapReady = false;

        mapRoot.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            if (!aspectApplied)
            {
                ApplyAspectRatioFit();
                _mapReady = true;
            }
        });

        waypointSpawner = new WaypointSpawner(waypointTemplateUXML, waypointContainer);


        if (playerIconTexture != null)
        {
            playerMarker.style.backgroundImage = new StyleBackground(playerIconTexture);
        }
        else
        {
            Debug.LogWarning("[MapOverlayController] No player icon texture assigned.");
        }

        if (mapTexture != null)
        {
            mapImage.style.backgroundImage = new StyleBackground(mapTexture);
        }
        else
        {
            Debug.LogWarning("[MapOverlayController] No mapTexture assigned.");
        }

        if (mapUIDocument != null && mapUIDocument.panelSettings != null)
            mapUIDocument.panelSettings.sortingOrder = 10;

        if (gpsProvider != null)
            gpsProvider.OnLocationChanged += UpdatePlayerPosition;

        if (mapUIDocument != null && mapUIDocument.panelSettings != null)
            mapUIDocument.panelSettings.sortingOrder = 10;

        if (!_bootstrapped)
            StartCoroutine(WarmupRoutine());

        // StartCoroutine(DelayedWaypointLoad());
    }

    private void OnDestroy()
    {
        if (gpsProvider != null)
            gpsProvider.OnLocationChanged -= UpdatePlayerPosition;
    }

    private void OnValidate()
    {
        if (mapUIDocument == null)
            mapUIDocument = GetComponent<UIDocument>();
    }

    IEnumerator WarmupRoutine(float timeoutSeconds = 1.0f)
    {
        var t0 = Time.realtimeSinceStartup;

        // 1) Wait for this UIDocument’s panel to exist
        var root = mapUIDocument != null ? mapUIDocument.rootVisualElement : null;
        if (root != null && root.panel == null)
        {
            EventCallback<GeometryChangedEvent> onGeom = null;
            onGeom = (e) =>
            {
                _panelReady = true;
                root.UnregisterCallback(onGeom);
            };
            root.RegisterCallback(onGeom);

            while (!_panelReady && (Time.realtimeSinceStartup - t0) < timeoutSeconds)
                yield return null;

            if (!_panelReady && root.panel != null) _panelReady = true;
        }
        else
        {
            _panelReady = (root != null && root.panel != null);
        }

        if (!_panelReady)
            Debug.LogWarning("[Map] Panel was not ready before timeout; continuing anyway.");

        // 2) Wait for database to be populated a few frames
        int tries = 0;
        while (tries++ < 30)
        {
            if (waypointDatabase != null && waypointDatabase.waypoints != null && waypointDatabase.waypoints.Count > 0)
                break;
            yield return null;
        }
        if (waypointDatabase == null || waypointDatabase.waypoints == null || waypointDatabase.waypoints.Count == 0)
            Debug.LogWarning("[Map] WaypointDatabase is empty at first show; map will open but without markers.");

        // 3) Spawn from DB (idempotent)
        EnsureWaypointsSpawnedFromDatabase();

        // 4) Refresh lock/completion states so icons match
        RefreshWaypointStates();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DevValidateDatabase();
#endif

        _bootstrapped = true;

        if (!_waypointsSpawned)
            StartCoroutine(WaitForMapAndWaypoints());
    }

    private void EnsureWaypointsSpawnedFromDatabase()
    {
        // 1️⃣ Wait until map is ready
        if (!_mapReady || mapImage.resolvedStyle.width <= 1f)
        {
            StartCoroutine(WaitForMapAndRetry());
            return;
        }

        // 2️⃣ Proceed normally
        if (_waypointsSpawned) return;
        if (waypointDatabase == null || waypointDatabase.waypoints == null || waypointDatabase.waypoints.Count == 0)
        {
            Debug.LogWarning("[Map] No waypoints to spawn.");
            return;
        }

        int spawned = 0;
        foreach (var w in waypointDatabase.waypoints)
            {
                if (w.type != WaypointData.WaypointType.Main) continue;
            float before = waypointContainer.childCount;
            AddWaypoint(w.latitude, w.longitude, w.id);
                if (waypointContainer.childCount > before) spawned++;
            }
        _waypointsSpawned = spawned > 0;
    }

    private IEnumerator WaitForMapAndRetry()
    {
        Debug.Log("[Map] Waiting for mapImage to become ready...");
        yield return new WaitUntil(() => _mapReady && mapImage.resolvedStyle.width > 1f);
        EnsureWaypointsSpawnedFromDatabase();
    }
    private IEnumerator WaitForMapAndWaypoints()
    {
        // Wait for layout
        while (mapImage == null || mapImage.resolvedStyle.width <= 1f)
            yield return null;

        // Wait for WaypointManager to populate
        while (waypointManager == null || !waypointManager.ContainsWaypoint("SO_GK"))
            yield return null; // waits until at least GK exists (first waypoint in DB)

        Debug.Log("[MapOverlayController] 🟢 Map and WaypointManager ready. Drawing icons...");
        int spawned = 0;
        foreach (var w in waypointManager.database.waypoints)
        {
            if (w.type != WaypointData.WaypointType.Main) continue;
            float before = waypointContainer.childCount;
            AddWaypoint(w.latitude, w.longitude, w.id);
                   if (waypointContainer.childCount > before) spawned++;
        }
        _waypointsSpawned = spawned > 0;
        RefreshWaypointStates();
    }



    private void UpdatePlayerPosition()
    {
        if (!gpsProvider.IsGpsAvailable) return;

        double lat = gpsProvider.Latitude;
        double lon = gpsProvider.Longitude;

        float u = Mathf.InverseLerp((float)lonLeft, (float)lonRight, (float)lon);
        float v = Mathf.InverseLerp((float)latBottom, (float)latTop, (float)lat);

        float px = u * mapImage.resolvedStyle.width;
        float py = (1f - v) * mapImage.resolvedStyle.height;        // flipped Y

        playerMarker.style.left = px;
        playerMarker.style.top = py;

        Debug.Log($"[MapOverlayController] Player at ({px:F1}, {py:F1}) from lat={lat:F6}, lon={lon:F6}");
    }

    public void AddWaypoint(double lat, double lon, string waypointId = "")
    {
       bool BoundsValid()
    => !double.IsNaN(latTop) && !double.IsNaN(latBottom) &&
       !double.IsNaN(lonLeft) && !double.IsNaN(lonRight) &&
       latTop > latBottom && lonRight > lonLeft;

        if (mapImage == null || mapImage.resolvedStyle.width <= 1f)
        {
            Debug.LogWarning("[MapOverlayController] Map not ready, deferring waypoint spawn.");
            return;
        }
        if (!BoundsValid())
        {
            Debug.LogError($"[MapOverlayController] Invalid GPS bounds. " +
                           $"latTop={latTop}, latBottom={latBottom}, lonLeft={lonLeft}, lonRight={lonRight}");
            return;
        }

        if (waypointContainer == null || mapImage == null || waypointSpawner == null)
            return;

        float u = Mathf.InverseLerp((float)lonLeft, (float)lonRight, (float)lon);
        float v = Mathf.InverseLerp((float)latBottom, (float)latTop, (float)lat);

        float x = u * mapImage.resolvedStyle.width;
        float y = (1f - v) * mapImage.resolvedStyle.height;

        var data = waypointManager.database?.GetById(waypointId);
        var labelText = data?.displayName ?? waypointId;

        Debug.Log($"[MapOverlayController] Spawning {waypointId} → lat={lat}, lon={lon}");


        // NEW: pass (id, label, pos) and receive the *wrapper*
        var wrapper = waypointSpawner.SpawnWaypoint(waypointId, labelText, new Vector2(x, y));

        // Optional icon override
        var icon = wrapper.Q<VisualElement>("WaypointIcon");
        if (icon != null && waypointIconTexture != null)
            icon.style.backgroundImage = new StyleBackground(waypointIconTexture);

        var label = wrapper.Q<Label>("WaypointLabel");
        if (label != null && data != null)
            label.text = data.displayName;

        // Interactivity
        wrapper.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log($"[MapOverlay] Clicked waypoint: {waypointId}");

            if (icon != null && icon.ClassListContains("waypoint-selected"))
                return;

            if (missionManager != null && missionManager.IsWaypointLockedByCurrentMission(waypointId))
            {
                hintUIController?.ShowHint("Ova lokacija je već zadatak misije i ne može se izmeniti.", HintType.Warning);
                Debug.LogWarning($"[MapOverlay] Attempt to override mission waypoint blocked: {waypointId}");
                return;
            }

            // clear previous selection
            foreach (var child in waypointContainer.Children())
            {
                child.Q<VisualElement>("WaypointIcon")?.RemoveFromClassList("waypoint-selected");
                child.Q<Label>("WaypointLabel")?.RemoveFromClassList("waypoint-selected");
            }

            // select if not completed
            if (missionManager != null && !missionManager.IsWaypointCompleted(waypointId))
            {
                icon?.AddToClassList("waypoint-selected");
                label?.AddToClassList("waypoint-selected");
            }

            // set target via ID-safe overload
            if (waypointManager != null)
            {
                var target = waypointManager.database?.GetById(waypointId);
                if (target != null)
                {
                    gpsUI?.SetActiveTarget(target, WaypointTargetSource.Manual);
                }
                else
                {
                    Debug.LogWarning($"[MapOverlay] Waypoint not found in database: {waypointId}");
                }
            }
            else
            {
                Debug.LogWarning("[MapOverlay] WaypointManager is null");
            }

            if (float.IsNaN(u) || float.IsNaN(v))
            {
                Debug.LogError($"[MapOverlayController] u/v NaN for waypointId={waypointId}. " +
                               $"lat={lat}, lon={lon}. Bounds: top={latTop}, bottom={latBottom}, left={lonLeft}, right={lonRight}");
                return;
            }
        });
    }


    private bool aspectApplied = false;

    private void ApplyAspectRatioFit()
    {
        if (aspectApplied || mapImage == null) return;
        aspectApplied = true;

        float ratio = mapImageHeight / mapImageWidth;
        float screenWidth = mapRoot.resolvedStyle.width;
        float calculatedHeight = screenWidth * ratio;

        mapImage.style.width = screenWidth;
        mapImage.style.height = calculatedHeight;

        Debug.Log($"[MapOverlayController] Map scaled to width: {screenWidth}, height: {calculatedHeight} (aspect {ratio:F3})");
    }

    private System.Collections.IEnumerator DelayedWaypointLoad()
    {
        // Wait for one frame so layout completes
        yield return null;

        // Wait until mapImage has a valid size
        while (mapImage.resolvedStyle.width <= 1f)
            yield return null;

        yield return null;
        while (mapImage.resolvedStyle.width <= 1f)
            yield return null;

        if (_waypointsSpawned) yield break;

        // Wait until WaypointManager has runtime waypoints
        Dictionary<string, WaypointRuntime> waypoints = null;
        while (waypoints == null || waypoints.Count == 0)
        {
            var field = typeof(WaypointManager).GetField("activeWaypoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            waypoints = field?.GetValue(waypointManager) as Dictionary<string, WaypointRuntime>;
            yield return null;
        }

        Debug.Log("[MapOverlayController] 🟢 WaypointManager and layout ready. Drawing map icons...");
        foreach (var wp in waypoints.Values)
        {
            var data = waypointManager.database?.GetById(wp.waypointId);
            if (data == null)
            {
                Debug.LogWarning($"[MapOverlay] No database entry found for runtime waypoint: {wp.waypointId}");
                continue;
            }

            if (data.type != WaypointData.WaypointType.Main)
            {
                Debug.Log($"[MapOverlay] Skipping non-main waypoint: {data.displayName}");
                continue;
            }

            AddWaypoint(data.latitude, data.longitude, data.id);
        }

    }

    public void RefreshWaypointStates()
    {
        foreach (var wrapper in waypointContainer.Children())
        {
            var id = wrapper.name.Replace("WaypointWrapper_", "");
            bool isDone = missionManager.IsWaypointCompleted(id);

            var icon = wrapper.Q<VisualElement>("WaypointIcon");
            var label = wrapper.Q<Label>("WaypointLabel");

            if (isDone)
            {
                icon?.AddToClassList("waypoint-completed");
                label?.AddToClassList("waypoint-completed");
                wrapper.pickingMode = PickingMode.Ignore;
            }
            else
            {
                icon?.RemoveFromClassList("waypoint-completed");
                label?.RemoveFromClassList("waypoint-completed");
                wrapper.pickingMode = PickingMode.Position; // re-enable
            }
        }
    }

    public void ToggleMapOverlay()
    {
        if (!_bootstrapped)
            StartCoroutine(WarmupRoutine());
        mapVisible = !mapVisible;
        mapRoot.style.display = mapVisible ? DisplayStyle.Flex : DisplayStyle.None;
        mapRoot.pickingMode = mapVisible ? PickingMode.Position : PickingMode.Ignore; // add this
        Debug.Log($"[MapOverlay] Toggle (now {(mapVisible ? "ON" : "OFF")}) on instance {GetInstanceID()}");

    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void DevValidateDatabase()
    {
        if (waypointDatabase == null) return;

        var set = new HashSet<string>();
        foreach (var w in waypointDatabase.waypoints)
        {
            if (string.IsNullOrWhiteSpace(w.id))
                Debug.LogError("[Map] Waypoint with empty ID.");
            else if (!set.Add(w.id))
                Debug.LogError($"[Map] Duplicate waypoint ID: {w.id}");
        }
    }
}
