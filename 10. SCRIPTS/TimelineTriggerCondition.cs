using System;
using UnityEngine;

[Serializable]
public enum TriggerType
{
    None = 0,
    Time = 1,
    Waypoint = 2,
    Dialog = 3,
    POI = 4,
    UI = 5,
    Mission = 6,
    GameBlock = 7,
    TrackedImage = 8
}

[Serializable]
public class TimelineTriggerCondition
{
   // [Header("General")]
    public string label = "Trigger Description";

    public string conditionId;

  // [Header("Trigger Source")]
    public TriggerType triggerType;

    [Tooltip("Used if triggerType is Waypoint")]
    public string conWaypointId;
    public WaypointZoneLevel requiredZone = WaypointZoneLevel.Zone3;

    [Tooltip("Used if triggerType is Mission")]
    public string missionName;

    [Tooltip("Used if triggerType is GameBlock")]
    public string blockId;

    [Tooltip("Used if triggerType is TrackedImage")]
    public string imageName;

   // [Header("Event Details")]
    [Tooltip("Used if triggerType is Dialog")]
    public string dialogId;

    [Tooltip("Used if triggerType is POI")]
    public string poiId;

    [Tooltip("Used if triggerType is UI")]
    public string uiElementId;

    public TriggerSubEventType subEventType = TriggerSubEventType.None;

   // [Header("Timing")]
    public float requiredSeconds = 0f;
    public bool enableLocationCheck = false;

   // [Header("Options")]
    public bool triggerOnlyOnce = true;
    public bool allowInterrupt = false;
    public bool requireAllConditions = true;

    // [Header("Runtime State")]
    [NonSerialized] public bool hasTriggered = false;
}
