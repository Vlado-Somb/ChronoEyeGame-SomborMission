using System.Collections;
using UnityEngine;
using MyGame.Timeline;

[CreateAssetMenu(menuName = "Timeline/Delay Block")]
public class DelayBlockSO : TimelineBlockSO
{
    [Tooltip("How long to wait before completing this block (in seconds).")]
    public float delaySeconds = 2f;

    private bool isRunning = false;
    private Coroutine delayCoroutine;

    public override void StartBlock()
    {
        if (delaySeconds <= 0f)
        {
            if (verbose) Debug.Log("[DelayBlock] Delay is 0 or less. Skipping.");
            CompleteBlock();
            return;
        }

        if (verbose) Debug.Log($"[DelayBlock] Waiting {delaySeconds} seconds...");
        isRunning = true;
        delayCoroutine = manager.StartCoroutine(WaitAndComplete());
    }

    private IEnumerator WaitAndComplete()
    {
        float elapsed = 0f;
        while (elapsed < delaySeconds && isRunning)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isRunning)
        {
            if (verbose) Debug.Log("[DelayBlock] Done waiting.");
            CompleteBlock();
        }
    }

    public override void PauseBlock()
    {
        isRunning = false;
        if (verbose) Debug.Log("[DelayBlock] Paused.");
    }

    public override void ResumeBlock()
    {
        if (!isRunning)
        {
            if (verbose) Debug.Log("[DelayBlock] Resumed. Restarting delay...");
            isRunning = true;
            delayCoroutine = manager.StartCoroutine(WaitAndComplete());
        }
    }

    public override void StopBlock()
    {
        if (delayCoroutine != null)
        {
            manager.StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        isRunning = false;
        if (verbose) Debug.Log("[DelayBlock] Stopped.");
    }

    public override void ResetBlock()
    {
        isRunning = false;
        delayCoroutine = null;
    }

    private bool verbose => manager != null && manager.verboseLogging;
}
