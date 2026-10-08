using UnityEngine;
using UnityEngine.UIElements;
using System;

public class DialogUIController : MonoBehaviour
{
    [Header("Dialog UI Document")]
    [SerializeField] private UIDocument uiDoc;

    // Toggle for debug logging
    [SerializeField] private bool debugLogging = false;

    // publicly exposed
    public VisualElement overlay { get; private set; }
    public Label speakerLabel { get; private set; }
    public Label bodyLabel { get; private set; }
    public Button nextButton { get; private set; }
    public Button choiceA { get; private set; }
    public Button choiceB { get; private set; }
    public Label choiceALabel { get; private set; }
    public Label choiceBLabel { get; private set; }
    public Button missionButton { get; private set; }

    public event Action OnMissionStart;

    void Awake()
    {
        if (uiDoc == null) uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        // Prevent the very root from capturing any clicks
        root.pickingMode = PickingMode.Ignore;

        overlay = root.Q<VisualElement>("DialogOverlay");
        speakerLabel = root.Q<Label>("SpeakerText");
        bodyLabel = root.Q<Label>("BodyText");
        nextButton = root.Q<Button>("NextButton");
        choiceA = root.Q<Button>("ChoiceA");
        choiceB = root.Q<Button>("ChoiceB");
        choiceALabel = root.Q<Label>("ChoiceALabel");
        choiceBLabel = root.Q<Label>("ChoiceBLabel");
        missionButton = root.Q<Button>("MissionButton");


        if (debugLogging)
        {
            Debug.Log($"[DialogUIController] overlay       = {(overlay != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] speakerLabel  = {(speakerLabel != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] bodyLabel     = {(bodyLabel != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] nextButton    = {(nextButton != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] choiceA       = {(choiceA != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] choiceB       = {(choiceB != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] choiceALabel  = {(choiceALabel != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] choiceBLabel  = {(choiceBLabel != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogUIController] missionButton = {(missionButton != null ? "OK" : "NULL")}");

            // 1a) Log every pointer‐down that hits the root of the tree

            root.RegisterCallback<PointerDownEvent>(evt => {
                Debug.Log($"[DialogueUIController DEBUG] ROOT caught PointerDown — evt.target={evt.target}, evt.currentTarget={evt.currentTarget}");
            }, TrickleDown.TrickleDown); 

            // 1b) Log every pointer‐down that hits your overlay
            overlay.RegisterCallback<PointerDownEvent>(evt => {
                Debug.Log($"[DialogueUIController DEBUG] OVERLAY caught PointerDown — target={evt.target}, currentTarget={evt.currentTarget}");
            }, TrickleDown.TrickleDown); 

            // 1c) Log every pointer‐down that hits the NextButton
            nextButton.RegisterCallback<PointerDownEvent>(evt => {
                Debug.Log($"[DialogueUIController DEBUG] NEXTBUTTON caught PointerDown — target={evt.target}, currentTarget={evt.currentTarget}");
            });
        }

        var taskWrapper = root.Q<VisualElement>("task-ui-wrapper");
        if (taskWrapper != null)
            taskWrapper.pickingMode = PickingMode.Ignore;
        overlay.pickingMode = PickingMode.Ignore;
        foreach (var child in overlay.Children())
            child.pickingMode = PickingMode.Ignore;

        // Then re‐enable your buttons below:
        nextButton.pickingMode = PickingMode.Position;
        choiceA.pickingMode = PickingMode.Position;
        choiceB.pickingMode = PickingMode.Position;
        missionButton.pickingMode = PickingMode.Position;


        // hook up clicks
        missionButton.clicked += OnMissionButtonClicked;

        // hide *all* controls at start
        overlay.style.display = DisplayStyle.None;
        overlay.pickingMode = PickingMode.Ignore;
        nextButton.style.display = DisplayStyle.None;
        choiceA.style.display = DisplayStyle.None;
        choiceB.style.display = DisplayStyle.None;
        missionButton.style.display = DisplayStyle.None;
    }

    // call this from your DialogueManager when the whole convo is done
    public void ShowMissionButton()
    {
        // hide everything else in the overlay
        nextButton.style.display = DisplayStyle.None;
        choiceA.style.display = DisplayStyle.None;
        choiceB.style.display = DisplayStyle.None;
        speakerLabel.style.display = DisplayStyle.None;
        bodyLabel.style.display = DisplayStyle.None;

        // show the mission‐start button
        missionButton.style.display = DisplayStyle.Flex;

    }

    private void OnMissionButtonClicked()
    {
        // hide the entire overlay (optional)
        overlay.style.display = DisplayStyle.None;

        // fire your mission‐start logic
        OnMissionStart?.Invoke();
    }

    // helper to pull the overlay back in (you probably already have this)
    public void ShowOverlay() => overlay.style.display = DisplayStyle.Flex;
    public void HideOverlay() => overlay.style.display = DisplayStyle.None;
}
