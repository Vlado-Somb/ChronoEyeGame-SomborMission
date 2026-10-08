using UnityEngine;

public class TestDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogAsset firstAsset;  // assign Ducic Embassy asset

    void Start()
    {
        if (DialogueManager.Instance != null && firstAsset != null)
        {
            DialogueManager.Instance.StartDialogue(firstAsset);
        }
        else Debug.LogError("Missing DialogueManager.Instance or firstAsset");
    }
}
