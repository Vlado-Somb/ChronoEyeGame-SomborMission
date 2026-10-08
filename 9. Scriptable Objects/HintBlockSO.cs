using System.Collections;
using UnityEngine;
using MyGame.Timeline;

[CreateAssetMenu(menuName = "Timeline/Hint Block")]
public class HintBlockSO : TimelineBlockSO
{
    [Tooltip("Main text to show in the hint overlay.")]
    [TextArea(2, 4)]
    public string hintText = "You are close to the Embassy.";

    [Tooltip("Type of hint — affects color, title, and sound.")]
    public HintType hintType = HintType.Info;

    [Tooltip("Delay before showing the hint.")]
    public float delayBeforeShow = 0f;

    [Tooltip("Additional seconds to keep this hint on screen, after autoHideSeconds.")]
    public float extraDisplayTime = 0f;

    private Coroutine hintRoutine;

    public override void StartBlock()
    {
        if (string.IsNullOrWhiteSpace(hintText))
        {
            Debug.LogWarning("[HintBlockSO] No hint text provided. Completing immediately.");
            CompleteBlock();
            return;
        }

        hintRoutine = manager.StartCoroutine(PlayHint());
    }

    private IEnumerator PlayHint()
    {
        if (delayBeforeShow > 0f)
            yield return new WaitForSeconds(delayBeforeShow);

        if (verbose) Debug.Log($"[HintBlockSO] Queuing hint: {hintText} ({hintType})");

        HintUIController hintUI = Object.FindFirstObjectByType<HintUIController>();
        if (hintUI != null)
        {
            hintUI.ShowHint(hintText, hintType);
        }
        else
        {
            Debug.LogWarning("[HintBlockSO] No HintUIController found in scene.");
        }

        CompleteBlock();
    }

    public override void PauseBlock()
    {
        if (hintRoutine != null)
            manager.StopCoroutine(hintRoutine);
    }

    public override void ResumeBlock()
    {
        hintRoutine = manager.StartCoroutine(PlayHint());
    }

    public override void StopBlock()
    {
        if (hintRoutine != null)
        {
            manager.StopCoroutine(hintRoutine);
            hintRoutine = null;
        }
    }

    public override void ResetBlock()
    {
        hintRoutine = null;
    }

    private bool verbose => manager != null && manager.verboseLogging;
}
