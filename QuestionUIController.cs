using UnityEngine;
using UnityEngine.UIElements;
using System;

public class QuestionUIController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    // Event you’ll invoke when the user answers.
    public event Action<bool> OnAnswered;

    public void ShowQuestion(
        string questionText,
        string choiceALabel,
        string choiceBLabel,
        int correctIndex,
        Action<bool> onAnswered)
    {
        // TODO: wire this up to your UXML/USS question panel.
        Debug.Log($"[QuestionUI] Q:{questionText} A:{choiceALabel}/{choiceBLabel} (correct={correctIndex})");
        OnAnswered = onAnswered;
        // Show the panel, set labels, etc.
    }
    private VisualElement _root;

    void Awake()
    {
        if (uiDocument == null)
            Debug.LogError("QuestionUIController: no UIDocument assigned!");
        else
            _root = uiDocument.rootVisualElement;
    }

    public void Hide()
    {
        // TODO: hide your question panel
    }

    public void ShowRetry()
    {
        // TODO: flash “Try again” message
    }
}
