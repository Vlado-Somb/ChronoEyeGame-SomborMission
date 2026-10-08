public enum TriggerSubEventType
{
    None,

    // Dialog Events
    DialogStarted,
    DialogCompleted,

    // POI Events
    POIVisible,
    POIInteracted,
    POIActivated,

    // Timers
    GlobalTime,
    BlockTime,

    // Mission Events
    OnMissionStarted,
    OnMissionComplete,

    // Game-Block Events
    OnBlockStarted,
    OnBlockEnded,

    // Image-tracking events
    OnImageTracked,
    OnImageLost
}
