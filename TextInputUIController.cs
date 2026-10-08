using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TextInputUIController : MonoBehaviour
{
    public UIDocument uiDocument;      // Assign your UXML asset here
    private VisualElement root;
    private TextField inputField;
    private Button submitButton;
    private Label errorLabel;

    private Action<string> _onSubmit;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement.Q<VisualElement>("TextInputPanel");
        inputField = root.Q<TextField>("AnswerField");
        submitButton = root.Q<Button>("SubmitButton");
        errorLabel = root.Q<Label>("ErrorText");

        submitButton.clicked += () =>
        {
            _onSubmit?.Invoke(inputField.text);
        };

        Hide();
    }

    /// <summary>
    /// Show the input panel with placeholder text and capture callback.
    /// </summary>
    public void ShowInput(string placeholder, Action<string> onSubmit)
    {
        _onSubmit = onSubmit;
        inputField.value = "";
        // set the greyed-out hint text
        inputField.textEdition.placeholder = placeholder;
        // hide the hint once they start typing
        inputField.textEdition.hidePlaceholderOnFocus = true;
        errorLabel.text = "";
        errorLabel.style.display = DisplayStyle.None;

        root.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Hide the input panel.
    /// </summary>
    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Display an error message (e.g. “Try again!”).
    /// </summary>
    public void ShowError(string message)
    {
        errorLabel.text = message;
        errorLabel.style.display = DisplayStyle.Flex;
    }
}
