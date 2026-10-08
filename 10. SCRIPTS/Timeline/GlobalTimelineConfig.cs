using System.Collections.Generic;
using UnityEngine;
using MyGame.Timeline;

[CreateAssetMenu(menuName = "GameData/Global Timeline Config")]
public class GlobalTimelineConfig : ScriptableObject
{
    public List<TimelineSettings> allTimelines;

    public TimelineSettings GetActiveTimelineSettings()
    {
        return allTimelines != null && allTimelines.Count > 0
            ? allTimelines[0]
            : null;
    }

}
