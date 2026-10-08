// WaypointTriggerBinding.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Waypoint;
using MyGame.Timeline;

#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(menuName = "GameData/Waypoint Trigger Binding")]
public class WaypointTriggerBinding : ScriptableObject
{
    // Root-level triggers (if used) should always be initialized
    public List<TimelineTriggerCondition> globalTriggers = new List<TimelineTriggerCondition>();

    [Serializable]
    public class Binding
    {
        public string bindingLabel;

        public string groupId;

        [Tooltip("The main waypoint this bundle is bound to")]
        public string waypointId;

        [Tooltip("Zone level required to start monitoring this bundle (typically Zone1)")]
        public WaypointZoneLevel activationZone = WaypointZoneLevel.Zone1;

        [Tooltip("Target timeline block to fire when all conditions are met")]
        public int targetBlockIndex = -1;

        // Ensure nested triggers list is never null
        public List<TimelineTriggerCondition> triggers = new List<TimelineTriggerCondition>();
    }
    [Header("Waypoint Settings (optional override)")]
    [Tooltip("If non-null, this Settings asset overrides the global one.")]
    public WaypointSettings waypointSettings;
    [Header("Timeline Settings (optional override)")]
    [Tooltip("If non-null, this Settings asset overrides the global one.")]
    public TimelineSettings timelineSettings;

    // Main list of waypoint bindings
    public List<Binding> waypointTriggers = new List<Binding>();
}
