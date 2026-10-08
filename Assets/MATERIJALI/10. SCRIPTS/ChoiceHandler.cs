using UnityEngine;

public class ChoiceHandler : MonoBehaviour
{
    // This method will be called by the UnityEvent<int> onChoice
    // `choiceIndex` will be 0 for the first button, 1 for the second.
    public void HandleChoice(int choiceIndex)
    {
        Debug.Log($"ChoiceHandler.HandleChoice called with index {choiceIndex}");

        switch (choiceIndex)
        {
            case 0:
                OnChoiceZero();
                break;
            case 1:
                OnChoiceOne();
                break;
            default:
                Debug.LogWarning($"Unhandled choice index: {choiceIndex}");
                break;
        }
    }

    private void OnChoiceZero()
    {
        Debug.Log("Player chose option A");
        // TODO: add your branch‐A logic here
    }

    private void OnChoiceOne()
    {
        Debug.Log("Player chose option B");
        // TODO: add your branch‐B logic here
    }
}
