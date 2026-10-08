using UnityEngine;
using MyGame.Timeline;

/// <summary>
/// A timeline-driven dialog block that plays a DialogAsset via DialogueManager.StartStandaloneDialogue,
/// then advances the timeline when complete.
/// </summary>
[CreateAssetMenu(menuName = "Timeline/Standalone Dialog Block SO")]
public class TimelineStandaloneDialogBlockSO : TimelineBlockSO
{
    [Tooltip("The DialogAsset to play for this block.")]
    public DialogAsset dialogueAsset;

    public override void StartBlock()
    {
        if (dialogueAsset == null || DialogueManager.Instance == null)
        {
            Debug.LogWarning($"[TimelineStandalone] Missing asset or DialogueManager on '{name}'");
            CompleteBlock();
            return;
        }

        Debug.Log($"[TimelineStandalone] Starting dialogue '{dialogueAsset.name}' from block '{name}'");
        DialogueManager.Instance.StartStandaloneDialogue(dialogueAsset);
        DialogueManager.Instance.OnDialogueTimelineComplete.AddListener(OnTimelineDialogueComplete);
    }

    private void OnTimelineDialogueComplete()
    {
        Debug.Log($"[TimelineStandalone] OnTimelineComplete received for block '{name}'");
        DialogueManager.Instance.OnDialogueTimelineComplete.RemoveListener(OnTimelineDialogueComplete);
        CompleteBlock();
    }

    public override void StopBlock()
    {
        Debug.Log($"[TimelineStandalone] StopBlock '{name}', cleaning up listener");
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueTimelineComplete.RemoveListener(OnTimelineDialogueComplete);
    }
}
