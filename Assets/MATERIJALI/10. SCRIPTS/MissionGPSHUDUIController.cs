using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum WaypointTargetSource
{
    Mission,
    Manual,
    System
}


[RequireComponent(typeof(UIDocument))]
public class MissionGPSHUDUIController : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public WaypointDatabase waypointDatabase;

    [Header("GPS Provider")]
    public MonoBehaviour gpsProviderComponent;

    [Header("UI Document")]
    public UIDocument uiDoc;

    [SerializeField] 
    private MapOverlayController mapOverlayController;

    private VisualElement _root;
    private Label _lat, _lon, _alt, _dist, _waypointName, _globalTime, _blockTime;
    private Label _pointsLabel = null;

    private IGpsProvider _gps;
    private MasterTimelineManager _timeline;
    private WaypointManager _waypointManager;
    private ScoreManager _scoreMgr;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private HintUIController hintUIController;

    private bool _missionMode = false;
    private double _missionLat, _missionLon;
    private string _lastClosestWaypointId;
    private string _lastTargetName;

    private float _lastLocationUpdateTime = 0f;
    private int _lastGlobalSeconds = -1;
    private int _lastBlockSeconds = -1;

    public float CurrentDistance { get; private set; }
    public int GlobalSeconds => _lastGlobalSeconds;
    public int LastBlockSeconds => _lastBlockSeconds;

    void Awake()
    {
        if (uiDoc == null) uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        _timeline = FindFirstObjectByType<MasterTimelineManager>();
        _waypointManager = FindFirstObjectByType<WaypointManager>();


        _lat = _root.Q<Label>("LatValue");
        _lon = _root.Q<Label>("LonValue");
        _alt = _root.Q<Label>("AltValue");
        _globalTime = _root.Q<Label>("GlobalTimeValue");
        _blockTime = _root.Q<Label>("BlockTimeValue");
        _waypointName = _root.Q<Label>("WaypointName");
        _dist = _root.Q<Label>("DistanceValue");
        _pointsLabel = _root.Q<Label>("PointsValue");

        _scoreMgr = ScoreManager.Instance;
        if (_scoreMgr != null && _scoreMgr.IsScoringEnabled && _pointsLabel != null)
        {
            UpdatePointUI(_scoreMgr.CurrentScore);
            _scoreMgr.OnScoreChanged += UpdatePointUI;

        }
        else if (_pointsLabel != null)
        {
            _pointsLabel.style.display = DisplayStyle.None;
        }

        _root.style.display = DisplayStyle.Flex;


        if (gpsProviderComponent != null)
            _gps = gpsProviderComponent as IGpsProvider;

        if (_gps == null)
        {
            Debug.LogError("[MissionGPSHUDUIController] GPS Provider not assigned or invalid!");

            enabled = false;
            return;
        }

        _gps.OnLocationChanged += UpdateLocationUI;
        _timeline.OnBlockStarted += HandleNewBlock;
        _waypointManager.OnWaypointZoneEntered += HandleWaypointZoneEntered;

        UpdateLocationUI(); // Force sync on awake

        var root = GetComponent<UIDocument>().rootVisualElement;
        Button toggleButton = root.Q<Button>("ToggleMapButton");

        if (toggleButton != null && mapOverlayController != null)
        {
            toggleButton.clicked += () =>
            {
                mapOverlayController.ToggleMapOverlay();
            };
        }

    }

    void OnDestroy()
    {
        if (_gps != null)
            _gps.OnLocationChanged -= UpdateLocationUI;
        if (_timeline != null)
            _timeline.OnBlockStarted -= HandleNewBlock;
        if (_waypointManager != null)
            _waypointManager.OnWaypointZoneEntered -= HandleWaypointZoneEntered;
        if (_scoreMgr != null)
            _scoreMgr.OnScoreChanged -= UpdatePointUI;

    }

    void Update()
    {
        if (_timeline == null) return;

        int globalSec = (int)_timeline.totalTime;

        if (globalSec != _lastGlobalSeconds)
        {
            _lastGlobalSeconds = globalSec;
            TimeSpan gTs = TimeSpan.FromSeconds(globalSec);
            _globalTime.text = $"{(int)gTs.TotalHours:00}:{gTs.Minutes:00}:{gTs.Seconds:00}";
        }

        int blockSec = (int)_timeline.BlockElapsedTime;

        if (blockSec != _lastBlockSeconds)
        {
            _lastBlockSeconds = blockSec;
            TimeSpan bTs = TimeSpan.FromSeconds(blockSec);
            _blockTime.text = $"{bTs.Minutes:00}:{bTs.Seconds:00}";
        }
    }

    public void UpdatePointUI(int pts)
    {

        if (_pointsLabel == null) return;
        _pointsLabel.text = $"Poeni: {pts}";
        _pointsLabel.style.display = DisplayStyle.Flex;
    }

    private void UpdateLocationUI()
    {
        if (_root.style.display != DisplayStyle.Flex || _gps == null) return;

        EnsureLabelsBound();


        double lat = _gps.Latitude;
        double lon = _gps.Longitude;
        double alt = _gps.Altitude;

        _lat.text = lat.ToString("F6");
        _lon.text = lon.ToString("F6");
        _alt.text = $"{alt:F2} m";

        _lat.MarkDirtyRepaint();
        _lon.MarkDirtyRepaint();
        _alt.MarkDirtyRepaint();

        Debug.Log($"[GPS HUD] UI set → lat={lat}, lon={lon}, alt={alt}");


        // Throttle distance and waypoint scanning only
        if (Time.time - _lastLocationUpdateTime < 1f)
            return;

        _lastLocationUpdateTime = Time.time;

        if (_missionMode)
        {
            float d = HaversineUtils.CalculateDistanceMeters(lat, lon, _missionLat, _missionLon);

            CurrentDistance = d;
            _dist.text = $"{d:F0} m";
            return;
        }
        if (!_missionMode)
        {
            DisplayClosestWaypointFromDatabase(lat, lon);

        }
        else
        {
            _waypointName.text = "-";
            _dist.text = "0 m";
        }
    }

    private void DisplayClosestWaypointFromDatabase(double lat, double lon)
    {
        if (waypointDatabase == null || waypointDatabase.waypoints == null) return;

        float minDist = float.MaxValue;
        WaypointData closest = null;

        foreach (var wp in waypointDatabase.waypoints)
        {
            if (wp.type != WaypointData.WaypointType.Main) continue;
            float d = HaversineUtils.CalculateDistanceMeters(lat, lon, wp.latitude, wp.longitude);
            if (d < minDist)
            {
                minDist = d;
                closest = wp;
            }
        }

        if (closest != null)
        {
            if (_lastClosestWaypointId != closest.id)
            {
                _lastClosestWaypointId = closest.id;
                Debug.Log($"[MissionGPSHUDUIController] Nearest MAIN WP: {closest.displayName}, dist={minDist:F1}m");
            }

            CurrentDistance = minDist;
            _waypointName.text = closest.displayName;
            _dist.text = $"{minDist:F0} m";
        }
    }

    private void EnsureLabelsBound()
    {
        if (_lat == null) _lat = _root.Q<Label>("LatValue");
        if (_lon == null) _lon = _root.Q<Label>("LonValue");
        if (_alt == null) _alt = _root.Q<Label>("AltValue");
    }

    private void HandleNewBlock(ITimelineBlock block) => _blockTime.text = "00:00";

    private void HandleWaypointZoneEntered(string id, WaypointZoneLevel zone)
    {
        if (zone == WaypointZoneLevel.Zone2 && id == _lastClosestWaypointId)
        {
            Debug.Log($"[MissionGPSHUDUIController] ✅ Entered Zone2 for WP {id}");
        }
    }

    public void SetNextWaypointInfo(double lat, double lon, string name)
    {
        Debug.Log($"[GPS HUD] Next waypoint: {name} @ {lat:F6}, {lon:F6}");
    }

    // public void ClearMissionMode() => _missionMode = false;
    public void ClearMissionMode()
    {
        _missionMode = false;
        _waypointName.text = "-";
        _dist.text = "—";

    }

    public void UpdateDistance(float meters)
    {
        CurrentDistance = meters;
        _dist.text = $"{meters:F0} m";
    }


    private WaypointTargetSource _currentTargetSource;

  public void SetActiveTarget(double lat, double lon, string name, WaypointTargetSource source = WaypointTargetSource.Manual, bool overrideLocked = false)

    {
        ValidateLegacyCall(name);

        if (!overrideLocked && missionManager != null && missionManager.IsWaypointLockedByCurrentMission(name))
        {
            hintUIController?.ShowHint("Ne možete zameniti metu dok traje zadatak misije.", HintType.Warning);
            Debug.LogWarning($"[GPS HUD] Attempt to override locked mission waypoint via HUD blocked: {name}");
            return;
        }

        _missionMode = true; // Still needed for UpdateLocationUI to work
        _missionLat = lat;
        _missionLon = lon;
        _lastTargetName = name;
        _waypointName.text = name;
        _currentTargetSource = source;

        _waypointName.MarkDirtyRepaint();
        UpdateLocationUI(); // Optional: force immediate HUD refresh
        LogActiveTargetStatus();
    }

    public void SetActiveTarget(WaypointData wp, WaypointTargetSource source = WaypointTargetSource.Manual, bool overrideLocked = false)
    {
        if (!overrideLocked && missionManager != null && missionManager.IsWaypointLockedByCurrentMission(wp.id))
        {
            hintUIController?.ShowHint("Ne možete zameniti metu dok traje zadatak misije.", HintType.Warning);
            Debug.LogWarning($"[GPS HUD] Override blocked by mission lock: {wp.id}");
            return;
        }

        _missionMode = true;
        _missionLat = wp.latitude;
        _missionLon = wp.longitude;
        _lastTargetName = wp.displayName;
        _waypointName.text = wp.displayName;
        _currentTargetSource = source;
        UpdateLocationUI();
        LogActiveTargetStatus();
    }


    public void LogActiveTargetStatus()
    {
        Debug.Log($"[GPS HUD] Target set by: {_currentTargetSource} → {_lastTargetName} @ {_missionLat:F6}, {_missionLon:F6}");
    }


    public void Show() => _root.style.display = DisplayStyle.Flex;
    public void Hide() => _root.style.display = DisplayStyle.None;

    // --- DEV-ONLY LEGACY-CALL CHECK ---
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    void ValidateLegacyCall(string name)
    {
        if (waypointDatabase == null) return;

        bool isId = waypointDatabase.waypoints.Exists(w => w.id == name);
        if (!isId)
        {
            bool looksLikeDisplay = waypointDatabase.waypoints.Exists(w => w.displayName == name);
            var hint = looksLikeDisplay
                ? "Looks like a displayName, not an ID."
                : "Not found as an ID in WaypointDatabase.";

            Debug.LogError($"[GPS HUD] Legacy SetActiveTarget(lat,lon,name) called with non-ID name='{name}'. {hint}\n" +
                           ">> Fix usage: pass WaypointData to SetActiveTarget(WaypointData,...).\n" +
                           $"Caller: {new System.Diagnostics.StackTrace(1, true)}");
        }
    }
    public void ClearActiveTarget()
    {
        // Fully drop the current target and switch HUD out of mission mode.
        _missionMode = false;
        _missionLat = 0;
        _missionLon = 0;
        _lastTargetName = "-";
        _currentTargetSource = WaypointTargetSource.System;

        if (_waypointName != null) _waypointName.text = "-";
        if (_dist != null) _dist.text = "—";

        // Let the HUD immediately resume nearest-waypoint display
        UpdateLocationUI();
    }



}
