using UnityEngine;
using MyGame.Waypoint;

namespace MyGame.Timeline
{
    [CreateAssetMenu(menuName = "GameData/Timeline Settings")]
    public class TimelineSettings : ScriptableObject
    {
        public TimelineBlockDatabase timelineDatabase;
    }
}
