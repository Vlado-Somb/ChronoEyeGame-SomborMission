using UnityEngine;

public class StartTimelineOnLoad : MonoBehaviour
{
    [Tooltip("Reference to the MasterTimelineManager in the scene.")]
    public MasterTimelineManager timeline;

    [Tooltip("Delay before starting (optional, in seconds).")]
    public float startDelay = 0f;

    private void Start()
    {
        if (timeline == null)
        {
            timeline = FindFirstObjectByType<MasterTimelineManager>();
        }

        if (timeline == null)
        {
            Debug.LogError("[StartTimelineOnLoad] No MasterTimelineManager found!");
            return;
        }

        if (startDelay > 0f)
            Invoke(nameof(DelayedStart), startDelay);
        else
            timeline.StartTimeline();
    }

    private void DelayedStart()
    {
        timeline.StartTimeline();
    }
}
