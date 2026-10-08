// WaypointDatabase.cs
using System;                        // for [Serializable]
using System.Collections.Generic;   // for List<>
using System.Linq;
using UnityEngine;                   // for ScriptableObject

[CreateAssetMenu(menuName = "GameData/Waypoint Database")]
public class WaypointDatabase : ScriptableObject
{
    public List<WaypointData> waypoints = new();

    public WaypointData GetById(string id)
    {
        var wp = waypoints.Find(w => w.id == id);
        if (wp == null)
        {
            Debug.LogWarning($"[WaypointDatabase] Waypoint with ID '{id}' not found.");
        }
        return wp;
    }

    public string[] GetWaypointIds()
    {
        if (waypoints == null)
            return new string[0];

        return waypoints
            .Where(wp => wp != null && !string.IsNullOrEmpty(wp.id))
            .Select(wp => wp.id)
            .Distinct()
            .ToArray();
    }
}

[Serializable]
public class WaypointData
{
    public enum WaypointType
    {
        Main,
        Additional
    }

    public WaypointType type = WaypointType.Main;

    public string id = "new_waypoint";
    public string displayName = "New Waypoint";

    [Header("Geolocation")]
    public double latitude;
    public double longitude;
    public double altitude;
    [Header("Trigger Radii (meters)")]
    public float triggerRadiusLevel1 = 300f;
    public float triggerRadiusLevel2 = 100f;
    public float triggerRadiusLevel3 = 10f;

    [Header("Zone Confirmation Delays (sec)")]
    public float zone1ConfirmDelay = 0f;
    public float zone2ConfirmDelay = 0f;
    public float zone3ConfirmDelay = 2f;
}
