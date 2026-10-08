using UnityEngine;
using UnityEngine.Events;
using MyGame.Timeline;

/// <summary>
/// A mission/POI-injected dialog block that plays a DialogAsset via DialogueManager.StartInjectedDialogue,
/// then returns control to MissionManager when complete.
/// </summary>
[CreateAssetMenu(menuName = "Timeline/Injected Dialog Block SO")]
public class TimelineInjectedDialogBlockSO : TimelineBlockSO
{
    [Tooltip("The DialogAsset to play for this injected block.")]
    public DialogAsset dialogueAsset;

    public override void StartBlock()
    {
        if (dialogueAsset == null || DialogueManager.Instance == null)
        {
            Debug.LogWarning($"[TimelineInjected] Missing asset or DialogueManager on '{name}'");
            CompleteBlock();
            return;
        }

        Debug.Log($"[TimelineInjected] Starting injected dialogue '{dialogueAsset.name}' from block '{name}'");
        DialogueManager.Instance.StartInjectedDialogue(dialogueAsset, OnInjectedDialogueComplete);
    }

    private void OnInjectedDialogueComplete()
    {
        Debug.Log($"[TimelineInjected] OnInjectedComplete received for block '{name}'");
        CompleteBlock();
    }

    public override void StopBlock()
    {
        // No persistent listeners to remove, callback removed upon invocation
    }
}
