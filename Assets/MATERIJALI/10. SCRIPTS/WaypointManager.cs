// WaypointManager.cs — using WaypointRuntime + confirmed zone entry logic
using System;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointZoneLevel
{
    None,
    Zone1,
    Zone2,
    Zone3
}

public class WaypointManager : MonoBehaviour
{
    [Header("Sources")]
    public WaypointDatabase database;
    public bool useRuntimeWaypoints = false;

    public event Action<string, WaypointZoneLevel> OnWaypointZoneEntered;

    [Header("GPS Provider")]
    [Tooltip("Assign an IGpsProvider implementing MonoBehaviour (e.g., EditorGpsSimulator or RealGpsProvider)")]
    [SerializeField] private MonoBehaviour gpsProviderMono;
    private IGpsProvider gpsProvider;

    private readonly Dictionary<string, WaypointRuntime> activeWaypoints = new();

    private float checkInterval = 1f;
    private float nextCheckTime = 0f;

    private void Awake()
    {
        gpsProvider = gpsProviderMono as IGpsProvider;

        if (gpsProvider == null)
        {
            Debug.LogError("[WaypointManager] No valid IGpsProvider assigned.");
            return;
        }

        Debug.Log("[WaypointManager] ✅ GPS provider assigned: " + gpsProvider.GetType().Name);

        if (!useRuntimeWaypoints && database != null)
        {
            Debug.Log($"[WaypointManager] Initializing waypoints from database ({database.waypoints.Count} entries)");
            foreach (var wp in database.waypoints)
            {
                Debug.Log($"[WaypointManager] → Adding {wp.id} ({wp.displayName}) @ ({wp.latitude}, {wp.longitude})");
                AddWaypoint(new WaypointRuntime(wp));
            }
        }
        else
        {
            Debug.LogWarning("[WaypointManager] ⚠️ Waypoint database missing or runtime loading is enabled.");
        }
    }


    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime) return;
        nextCheckTime = Time.unscaledTime + checkInterval;

        CheckWaypoints();
    }

    public void AddWaypoint(WaypointRuntime wp)
    {
        if (!activeWaypoints.ContainsKey(wp.waypointId))
        {
            activeWaypoints[wp.waypointId] = wp;
            Debug.Log($"[WaypointManager] Runtime waypoint added: {wp.waypointId}");
        }
    }

    private void CheckWaypoints()
    {
        if (gpsProvider == null || !gpsProvider.IsGpsAvailable) return;

        double lat = gpsProvider.Latitude;
        double lon = gpsProvider.Longitude;

        foreach (var wp in activeWaypoints.Values)
        {
            float dist = HaversineUtils.CalculateDistanceMeters(lat, lon, wp.latitude, wp.longitude);
            wp.lastDistance = dist;

            WaypointZoneLevel zone = DetermineZone(dist, wp);
            WaypointZoneLevel previous = wp.lastZone;

            if (zone != previous)
            {
                wp.zoneTimers[zone] = Time.unscaledTime;
                wp.lastZone = zone;
                Debug.Log($"[WaypointManager] {wp.waypointId} → zone changed: {previous} → {zone}");
            }
            else if (zone != WaypointZoneLevel.None)
            {
                float enteredAt = wp.zoneTimers.TryGetValue(zone, out var t) ? t : -1f;
                float duration = Time.unscaledTime - enteredAt;

                float required = GetConfirmationDelay(zone, wp);
                if (!wp.zoneConfirmed.Contains(zone) && duration >= required)
                {
                    wp.zoneConfirmed.Add(zone);
                    Debug.Log($"[WaypointManager] ✅ Confirmed zone entry for {wp.waypointId} → {zone} ({dist:F1}m)");
                    OnWaypointZoneEntered?.Invoke(wp.waypointId, zone);
                }
            }
        }
    }

    private WaypointZoneLevel DetermineZone(float dist, WaypointRuntime wp)
    {
        if (dist <= wp.triggerRadiusLevel3) return WaypointZoneLevel.Zone3;
        if (dist <= wp.triggerRadiusLevel2) return WaypointZoneLevel.Zone2;
        if (dist <= wp.triggerRadiusLevel1) return WaypointZoneLevel.Zone1;
        return WaypointZoneLevel.None;
    }

    private float GetConfirmationDelay(WaypointZoneLevel zone, WaypointRuntime wp)
    {
        return zone switch
        {
            WaypointZoneLevel.Zone1 => wp.zone1ConfirmDelay,
            WaypointZoneLevel.Zone2 => wp.zone2ConfirmDelay,
            WaypointZoneLevel.Zone3 => wp.zone3ConfirmDelay,
            _ => 0f,
        };
    }

    public bool ContainsWaypoint(string id)
    {
        return activeWaypoints != null && activeWaypoints.ContainsKey(id);
    }

    public WaypointZoneLevel GetCurrentZoneLevelFor(string waypointId)
    {
        if (activeWaypoints.TryGetValue(waypointId, out var wp))
            return wp.lastZone;
        return WaypointZoneLevel.None;
    }
    public void RemoveWaypoint(string waypointId)
    {
        if (activeWaypoints.ContainsKey(waypointId))
        {
            activeWaypoints.Remove(waypointId);
            Debug.Log($"[WaypointManager] ❌ Removed waypoint: {waypointId}");
        }
        else
        {
            Debug.LogWarning($"[WaypointManager] ⚠️ Tried to remove nonexistent waypoint: {waypointId}");
        }
    }


    public void ClearAll()
    {
        Debug.Log("[WaypointManager] Clearing all runtime waypoints");
        activeWaypoints.Clear();
    }
}

[Serializable]
public class WaypointRuntime
{
    public string waypointId;
    public double latitude, longitude, altitude;
    public float triggerRadiusLevel1 = 300f;
    public float triggerRadiusLevel2 = 100f;
    public float triggerRadiusLevel3 = 10f;
    public float zone1ConfirmDelay = 2f;
    public float zone2ConfirmDelay = 2f;
    public float zone3ConfirmDelay = 2f;

    public float lastDistance;
    public WaypointZoneLevel lastZone = WaypointZoneLevel.None;
    public HashSet<WaypointZoneLevel> zoneConfirmed = new();
    public Dictionary<WaypointZoneLevel, float> zoneTimers = new();

    public WaypointRuntime() { } // ← 🟢 This makes object initializers work

    public WaypointRuntime(WaypointData data)
    {
        waypointId = data.id;
        latitude = data.latitude;
        longitude = data.longitude;
        altitude = data.altitude;
        triggerRadiusLevel1 = data.triggerRadiusLevel1;
        triggerRadiusLevel2 = data.triggerRadiusLevel2;
        triggerRadiusLevel3 = data.triggerRadiusLevel3;
        zone1ConfirmDelay = data.zone1ConfirmDelay;
        zone2ConfirmDelay = data.zone2ConfirmDelay;
        zone3ConfirmDelay = data.zone3ConfirmDelay;
    }
}


public static class HaversineUtils
{
    private const double EarthRadius = 6371000; // meters

    public static float CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = DegreeToRad(lat2 - lat1);
        double dLon = DegreeToRad(lon2 - lon1);

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(DegreeToRad(lat1)) * Math.Cos(DegreeToRad(lat2)) *
                   Math.Pow(Math.Sin(dLon / 2), 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (float)(EarthRadius * c);
    }

    private static double DegreeToRad(double deg) => deg * (Math.PI / 180.0);

}