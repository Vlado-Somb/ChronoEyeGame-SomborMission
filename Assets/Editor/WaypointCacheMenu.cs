using UnityEditor;
using UnityEngine;
using MyGame.Waypoint;

public static class WaypointCacheMenu
{
    [MenuItem("Tools/Waypoint/🔄 Refresh Waypoint Cache")]
    public static void RefreshCache()
    {
        WaypointCache.RefreshWaypointCache();
        Debug.Log($"✅ Waypoint cache refreshed. {WaypointCache.CachedWaypointIds.Length} entries loaded.");
    }
}
