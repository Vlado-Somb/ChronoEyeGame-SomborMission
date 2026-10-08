using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaypointTriggerGroup
{
    public string waypointId;

    public List<TimelineTriggerCondition> triggers = new();
}
