using UnityEngine;
using System.Collections;

/// <summary>
/// Orchestrates hint display based on TriggerEvaluator events.
/// Combines a global base duration (autoHideSeconds) with each hint's extraDisplayTime.
/// </summary>
public class HintOrchestrator : MonoBehaviour
{
    [Tooltip("Reference to the UI controller handling hint presentation.")]
    [SerializeField] private HintUIController hintUIController;

    private void OnEnable()
    {
        // Subscribe to hint triggers from TriggerEvaluator
        TriggerEvaluator.OnHintTriggered += HandleHintTriggered;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        TriggerEvaluator.OnHintTriggered -= HandleHintTriggered;
    }

    private void OnDestroy()
    {
        TriggerEvaluator.OnHintTriggered -= HandleHintTriggered;
    }

    /// <summary>
    /// Invoked when TriggerEvaluator fires a hint event.
    /// Starts the hint display sequence.
    /// </summary>
    /// <param name="hint">The HintBlockSO containing timing and text.</param>
    private void HandleHintTriggered(HintBlockSO hint)
    {
        StartCoroutine(ShowHintSequence(hint));
    }

    /// <summary>
    /// Coroutine to handle delayed show and custom duration.
    /// </summary>
    private IEnumerator ShowHintSequence(HintBlockSO hint)
    {
        // Optional delay before showing the hint
        if (hint.delayBeforeShow > 0f)
            yield return new WaitForSeconds(hint.delayBeforeShow);

        // Calculate total display time: base auto-hide + extra hint-specific time
        float totalDuration = hintUIController.autoHideSeconds + hint.extraDisplayTime;

        // Show the hint with override duration
        hintUIController.ShowHint(hint.hintText, hint.hintType, totalDuration);

        yield break;
    }
}
