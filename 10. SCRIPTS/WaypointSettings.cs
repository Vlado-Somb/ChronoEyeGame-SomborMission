using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Waypoint
{
    [CreateAssetMenu(menuName = "Waypoint/Settings")]
    public class WaypointSettings : ScriptableObject
    {
        [Tooltip("All available waypoint databases. The first matching one will be used.")]
        public List<WaypointDatabase> waypointDatabases = new List<WaypointDatabase>();
        // Optionally add a “defaultIndex” or a lookup key if you want to choose at runtime
        public int defaultIndex = 0;

        public WaypointDatabase GetDefaultDatabase()
        {
            if (waypointDatabases != null && waypointDatabases.Count > 0)
            {
                int index = Mathf.Clamp(defaultIndex, 0, waypointDatabases.Count - 1);
                return waypointDatabases[index];
            }
            return null;
        }
    }
}
