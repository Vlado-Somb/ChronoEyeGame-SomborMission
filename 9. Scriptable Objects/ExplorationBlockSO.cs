using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyGame.Timeline;

[CreateAssetMenu(menuName = "Timeline/Exploration Block")]
public class ExplorationBlockSO : TimelineBlockSO
{
    [Tooltip("Total free roam time before auto-advancing (in seconds).")]
    public float duration = 30f;

    [Tooltip("If true, player can skip exploration manually.")]
    public bool allowEarlyExit = true;

    [Tooltip("Optional key to trigger skip (for dev/editor use).")]
    public KeyCode skipKey = KeyCode.E;

    [Tooltip("Optional skip button (auto-hidden on complete).")]
    public GameObject skipButtonObject; // assign the UI Button's GameObject here

    private bool isRunning = false;
    private float elapsed = 0f;
    private Coroutine explorationCoroutine;

    public override void StartBlock()
    {
        if (duration <= 0f)
        {
            if (verbose) Debug.Log("[ExplorationBlock] Duration is 0 or negative. Skipping.");
            CompleteBlock();
            return;
        }

        if (verbose) Debug.Log($"[ExplorationBlock] Free roam started. Duration: {duration} sec");

        elapsed = 0f;
        isRunning = true;

        if (allowEarlyExit)
            EnableSkipButton(true);

        explorationCoroutine = manager.StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (elapsed < duration && isRunning)
        {
#if UNITY_EDITOR
            if (allowEarlyExit && Input.GetKeyDown(skipKey))
            {
                if (verbose) Debug.Log("[ExplorationBlock] Skip key pressed → Completing early.");
                break;
            }
#endif
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isRunning)
        {
            if (verbose) Debug.Log("[ExplorationBlock] Ending.");
            CompleteBlock();
        }
    }

    private void EnableSkipButton(bool show)
    {
        if (skipButtonObject != null)
        {
            skipButtonObject.SetActive(show);

            if (show)
            {
                var button = skipButtonObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(OnSkipPressed);
                }
            }
        }
    }

    private void OnSkipPressed()
    {
        if (verbose) Debug.Log("[ExplorationBlock] Skip button pressed → Completing early.");
        CompleteBlock();
    }

    public override void PauseBlock()
    {
        isRunning = false;
        if (verbose) Debug.Log("[ExplorationBlock] Paused.");
    }

    public override void ResumeBlock()
    {
        if (!isRunning)
        {
            if (verbose) Debug.Log("[ExplorationBlock] Resumed.");
            isRunning = true;
            explorationCoroutine = manager.StartCoroutine(Countdown());
        }
    }

    public override void StopBlock()
    {
        if (explorationCoroutine != null)
        {
            manager.StopCoroutine(explorationCoroutine);
            explorationCoroutine = null;
        }
        EnableSkipButton(false);
        isRunning = false;
        if (verbose) Debug.Log("[ExplorationBlock] Stopped.");
    }

    public override void ResetBlock()
    {
        EnableSkipButton(false);
        isRunning = false;
        elapsed = 0f;
        explorationCoroutine = null;
    }

    private bool verbose => manager != null && manager.verboseLogging;
}
