using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace MyGame.Waypoint
{
    public static class WaypointCache
    {

#pragma warning disable UDR0001 // Domain Reload Analyzer
        private static WaypointSettings _globalSettings;
#pragma warning restore UDR0001 // Domain Reload Analyzer
        public static WaypointSettings GlobalSettings
        {
            get
            {
                if (_globalSettings == null)
                {
                    // find your Settings asset once
                    var guids = AssetDatabase.FindAssets("t:WaypointSettings");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _globalSettings = AssetDatabase.LoadAssetAtPath<WaypointSettings>(path);
                    }
                    else
                    {
                        Debug.LogWarning("No WaypointSettings asset found in project.");
                    }
                }
                return _globalSettings;
            }
        }
        // assign once
#pragma warning disable UDR0001 // Domain Reload Analyzer
        private static WaypointSettings _settings;
#pragma warning restore UDR0001 // Domain Reload Analyzer

        // cached data
#pragma warning disable UDR0001 // Domain Reload Analyzer
        private static string[] _cachedWaypointIds;
#pragma warning restore UDR0001 // Domain Reload Analyzer
#pragma warning disable UDR0001 // Domain Reload Analyzer
        private static GUIContent[] _cachedWaypointLabels;
#pragma warning restore UDR0001 // Domain Reload Analyzer

        public static void RefreshWaypointCache()
        {
            // 1) lazy-load the settings asset
            if (_settings == null)
            {
                var settingGuids = AssetDatabase.FindAssets("t:WaypointSettings");
                if (settingGuids == null || settingGuids.Length == 0)
                {
                    Debug.LogWarning("No WaypointSettings asset found. Create one via Create → Waypoint → Settings.");
                    ClearCache();
                    return;
                }
                var settingsPath = AssetDatabase.GUIDToAssetPath(settingGuids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<WaypointSettings>(settingsPath);
            }

            // 2) validate list & index
            if (_settings.waypointDatabases == null || _settings.waypointDatabases.Count == 0)
            {
                Debug.LogWarning("WaypointSettings has no databases assigned.");
                ClearCache();
                return;
            }

            int idx = Mathf.Clamp(_settings.defaultIndex, 0, _settings.waypointDatabases.Count - 1);
            var db = _settings.waypointDatabases[idx];
            if (db == null)
            {
                Debug.LogWarning($"WaypointSettings database at index {idx} is null.");
                ClearCache();
                return;
            }

            // 3) populate cache from the selected DB
            _cachedWaypointIds = db.GetWaypointIds() ?? Array.Empty<string>();
            _cachedWaypointLabels = _cachedWaypointIds
                                    .Select(id => new GUIContent(id))
                                    .ToArray();
        }

        /// <summary>All waypoint IDs from the selected database (or empty if none).</summary>
        public static string[] CachedWaypointIds
        {
            get
            {
                if (_cachedWaypointIds == null) RefreshWaypointCache();
                return _cachedWaypointIds;
            }
        }

        /// <summary>GUIContent wrappers matching CachedWaypointIds.</summary>
        public static GUIContent[] CachedWaypointLabels
        {
            get
            {
                if (_cachedWaypointLabels == null) RefreshWaypointCache();
                return _cachedWaypointLabels;
            }
        }

        private static void ClearCache()
        {
            _cachedWaypointIds = Array.Empty<string>();
            _cachedWaypointLabels = Array.Empty<GUIContent>();
        }
    }
}