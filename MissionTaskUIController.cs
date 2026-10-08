using System;
<<<<<<< Updated upstream
using System.Collections.Generic;
=======
>>>>>>> Stashed changes
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MissionTaskUIController : MonoBehaviour
{
    [Header("UXML References")]
    [Tooltip("Drag your MissionTaskUI UI Document here")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement taskUIWrapper;
    private VisualElement headerContainer;
    private Label missionTitleLabel;
    private Label taskTitleLabel;
    private Label stepLabel;
    private Label instructionLabel;

    private VisualElement questionContainer;
    private Label questionTextLabel;
    private Button choiceAButton;
    private Label choiceALabel;
    private Button choiceBButton;
    private Label choiceBLabel;
    private Label choiceErrorLabel;

<<<<<<< Updated upstream
    // ==== Multi-choice dynamic support ====
    private VisualElement choicesHost;                 // container to hold dynamic buttons
    private readonly List<Button> _dynamicChoiceButtons = new();
    private readonly Dictionary<Button, (bool pressed, double t0)> _pressStates = new();
    // Stores the handler pair we register on each dynamic button
    private readonly Dictionary<Button,
        (EventCallback<PointerDownEvent> down, EventCallback<PointerUpEvent> up)> _dynHandlers
            = new Dictionary<Button, (EventCallback<PointerDownEvent>, EventCallback<PointerUpEvent>)>();

    private const float HOLD_THRESHOLD = 0.2f; // keep your existing threshold


=======
>>>>>>> Stashed changes
    private VisualElement inputContainer;
    private Label answerPlaceField;
    private TextField inputField;
    private Button submitButton;
    private Label inputErrorLabel;

    private Action<object> onQuestionAnswered;
    private Action<object> onTextSubmitted;
    private int correctChoiceIndex;

    private VisualElement taskCompletionContainer;
    private Label completionTitleLabel;
    private Label completionQuoteLabel;

    // Stored delegates for event subscription
    private Action showHeaderOnStart;
    private Action hideHeaderOnComplete;

    private struct PressTracker
    {
        public bool isPressed;
        public double pressStartTime;
    }
    private PressTracker choiceAPress;
    private PressTracker choiceBPress;
<<<<<<< Updated upstream
    // private const float HOLD_THRESHOLD = 0.2f; // 0.3 seconds
=======
    private const float HOLD_THRESHOLD = 0.2f; // 0.3 seconds
>>>>>>> Stashed changes

    private PressTracker submitPress;
    private EventCallback<PointerDownEvent> _OnSubmitPointerDown;
    private EventCallback<PointerUpEvent> _OnSubmitPointerUp;

    private EventCallback<PointerDownEvent> _choiceA_PointerDownHandler;
    private EventCallback<PointerUpEvent> _choiceA_PointerUpHandler;
    private EventCallback<PointerDownEvent> _choiceB_PointerDownHandler;
    private EventCallback<PointerUpEvent> _choiceB_PointerUpHandler;

<<<<<<< Updated upstream
    private Coroutine _subscribeCo;

=======
>>>>>>> Stashed changes

    void Awake()
    {
        // 1) Grab the UIDocument
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("MissionTaskUIController: Missing UIDocument!", this);
            enabled = false;
            return;
        }

        // 2) Grab the VisualElement root
        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("MissionTaskUIController: rootVisualElement is null!", this);
            enabled = false;
            return;
        }

        root.RegisterCallback<PointerDownEvent>(evt =>
        {
            var targetName = evt.target?.GetType().Name + " / "
                             + ((evt.target as VisualElement)?.name ?? "<no-name>");
            Debug.Log($"[DEBUG] root saw a PointerDownEvent on “evt.target”:  {targetName}");
        }, TrickleDown.TrickleDown);

<<<<<<< Updated upstream

        // ROOT WRAPPER CONTAINER ELEMENTS
        taskUIWrapper = root.Q<VisualElement>("taskUIWrapper");

        // HEADER CONTAINER ELEMENTS
        headerContainer = root.Q<VisualElement>("headerContainer");
=======
        // ROOT WRAPPER CONTAINER ELEMENTS
        taskUIWrapper = root.Q<VisualElement>("task-ui-wrapper");

        // HEADER CONTAINER ELEMENTS
        headerContainer = root.Q<VisualElement>("header-container");
>>>>>>> Stashed changes
        missionTitleLabel = root.Q<Label>("missionTitleLabel");
        taskTitleLabel = root.Q<Label>("taskTitleLabel");
        stepLabel = root.Q<Label>("stepLabel");
        instructionLabel = root.Q<Label>("instructionLabel");
        // QUESTION CONTAINER ELEMENTS
<<<<<<< Updated upstream
        questionContainer = root.Q<VisualElement>("questionContainer");
=======
        questionContainer = root.Q<VisualElement>("question-container");
>>>>>>> Stashed changes
        questionTextLabel = root.Q<Label>("questionTextLabel");
        choiceAButton = root.Q<Button>("choiceAButton");
        choiceALabel = root.Q<Label>("choiceALabel");
        choiceBButton = root.Q<Button>("choiceBButton");
        choiceBLabel = root.Q<Label>("choiceBLabel");
<<<<<<< Updated upstream
        choiceErrorLabel = root.Q<Label>("choiceErrorLabel");

        // Create or find a host for dynamic choices
        choicesHost = root.Q<VisualElement>("choicesHost");
        {
            choicesHost = new VisualElement { name = "choicesHost" };
            choicesHost.style.flexDirection = FlexDirection.Column;
            choicesHost.style.paddingTop = 2;
            choicesHost.style.paddingBottom = 2;
            questionContainer.Add(choicesHost);
        }


        // INPUT CONTAINER ELEMENTS
        inputContainer = root.Q<VisualElement>("textInputContainer");
        answerPlaceField = root.Q<Label>("answerPlaceField");
=======
        choiceErrorLabel = root.Q<Label>("ChoiceErrorLabel");
        // INPUT CONTAINER ELEMENTS
        inputContainer = root.Q<VisualElement>("textinput-container");
        answerPlaceField = root.Q<Label>("AnswerPlaceField");
>>>>>>> Stashed changes
        inputField = root.Q<TextField>("inputField");
        submitButton = root.Q<Button>("submitButton");
        inputErrorLabel = root.Q<Label>("inputErrorLabel");
        // TASK COMPLETION CONTAINER ELEMENTS
<<<<<<< Updated upstream
        taskCompletionContainer = root.Q<VisualElement>("taskCompletionContainer");
        completionTitleLabel = root.Q<Label>("completionTitleLabel");
        completionQuoteLabel = root.Q<Label>("completionQuoteLabel");
=======
        taskCompletionContainer = root.Q<VisualElement>("task-completion-container");
        completionTitleLabel = root.Q<Label>("completion-title-label");
        completionQuoteLabel = root.Q<Label>("completion-quote-label");
>>>>>>> Stashed changes

        choiceALabel.pickingMode = PickingMode.Ignore;
        choiceBLabel.pickingMode = PickingMode.Ignore;
        /* submitButton.Q<Label>().pickingMode = PickingMode.Ignore; */
        Debug.Log($"[UI] In Awake(): choiceAButton = {choiceAButton}, choiceALabel = {choiceALabel}");

        if (taskUIWrapper == null) Debug.LogError("[UIController] taskUIWrapper is NULL");
        if (headerContainer == null) Debug.LogError("[UIController] headerContainer is NULL");
        if (missionTitleLabel == null) Debug.LogError("[UIController] missionTitleLabel is NULL");
        if (taskTitleLabel == null) Debug.LogError("[UIController] taskTitleLabel is NULL");
        if (instructionLabel == null) Debug.LogError("[UIController] instructionLabel is NULL");
        if (stepLabel == null) Debug.LogError("[UIController] instructionLabel is NULL");

        if (questionContainer == null) Debug.LogError("[UIController] question-container is NULL");
        if (questionTextLabel == null) Debug.LogError("[UIController] questionTextLabel is NULL");
        if (choiceAButton == null) Debug.LogError("[UIController] choiceAButton is NULL");
        if (choiceBButton == null) Debug.LogError("[UIController] choiceBButton is NULL");
        if (choiceErrorLabel == null) Debug.LogError("[UIController] ChoiceErrorLabel is NULL");

        if (inputContainer == null) Debug.LogError("[UIController] textinput-container is NULL");
        if (answerPlaceField == null) Debug.LogError("[UIController] AnswerPlaceField is NULL");
        if (inputField == null) Debug.LogError("[UIController] inputField is NULL");
        if (submitButton == null) Debug.LogError("[UIController] submitButton is NULL");
        if (inputErrorLabel == null) Debug.LogError("[UIController] inputErrorLabel is NULL");

        if (taskCompletionContainer == null) Debug.LogError("[MissionTaskUIController] taskCompletionContainer is NULL");
        if (completionTitleLabel == null) Debug.LogError("[MissionTaskUIController] completionTitleLabel is NULL");
        if (completionQuoteLabel == null) Debug.LogError("[MissionTaskUIController] completionQuoteLabel is NULL");

        if (taskUIWrapper != null) taskUIWrapper.style.display = DisplayStyle.None;
        if (headerContainer != null) headerContainer.style.display = DisplayStyle.None;
        if (questionContainer != null) questionContainer.style.display = DisplayStyle.None;
        if (inputContainer != null) inputContainer.style.display = DisplayStyle.None;
        if (taskCompletionContainer != null) inputContainer.style.display = DisplayStyle.None;
    }

    void OnEnable()
    {
        var mgr = MissionManager.Instance;
        if (mgr != null)
        {
            // Create stored delegates
            showHeaderOnStart = () => ShowHeader(true);
            hideHeaderOnComplete = () => ShowHeader(false);

            // Subscribe to events
            mgr.OnMissionStarted += showHeaderOnStart;
            mgr.OnTaskChanged += UpdateTaskUI;
            mgr.OnMissionComplete += hideHeaderOnComplete;
        }
        else
        {
            Debug.LogWarning("[MissionTaskUI] Could not subscribe to MissionManager events (Instance not ready)");
        }
<<<<<<< Updated upstream
        TrySubscribeOrRetry();
=======
>>>>>>> Stashed changes
    }

    void OnDisable()
    {
<<<<<<< Updated upstream
        if (_subscribeCo != null) { StopCoroutine(_subscribeCo); _subscribeCo = null; }

        var mgr = MissionManager.Instance;
        if (mgr != null)
        {
=======
        var mgr = MissionManager.Instance;
        if (mgr != null)
        {
            // Unsubscribe stored delegates
>>>>>>> Stashed changes
            mgr.OnMissionStarted -= showHeaderOnStart;
            mgr.OnTaskChanged -= UpdateTaskUI;
            mgr.OnMissionComplete -= hideHeaderOnComplete;
        }
    }

    /// <summary>
    /// Show the entire Task UI (root container).
    /// </summary>
    public void Show() => root.style.display = DisplayStyle.Flex;

    /// <summary>
    /// Populates the entire header block:  
    /// • Mission name  
    /// • Task title (or fallback)  
    /// • Instruction text (or fallback)  
    /// • Step counter  
    /// </summary>
    /// 
    /// <summary>Show or hide the entire header block.</summary>
    public void ShowHeader(bool visible)
    {
        taskUIWrapper.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
        headerContainer.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    public void ShowWrapperContainerOnly(bool visible)
    {
        taskUIWrapper.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    /// <summary>Show or hide the multiple-choice section.</summary>
    public void ShowQuestionSection(bool visible)
<<<<<<< Updated upstream
    {
        questionContainer.style.display = visible
              ? DisplayStyle.Flex
              : DisplayStyle.None;
    }
    /// <summary>Show or hide the text-input section.</summary>
    public void ShowTextInputSection(bool visible)
    {
        inputContainer.style.display = visible
              ? DisplayStyle.Flex
              : DisplayStyle.None;
    }
    public void ShowCompletionSection(bool visible)
    {
        taskUIWrapper.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
        taskCompletionContainer.style.display = visible 
            ? DisplayStyle.Flex 
            : DisplayStyle.None;
    }

    public void SetTaskHeader(
        string missionName,
        string taskTitleOrFallback,
        string instructionOrFallback,
        int currentStep,
        int totalSteps)
    {
        missionTitleLabel.text = missionName;

        taskTitleLabel.text = string.IsNullOrWhiteSpace(taskTitleOrFallback)
            ? "Untitled Task"
            : taskTitleOrFallback;

        instructionLabel.text = string.IsNullOrWhiteSpace(instructionOrFallback)
            ? "No instruction."
            : instructionOrFallback;

        stepLabel.text = $"{currentStep}/{totalSteps}";
    }
=======
        => questionContainer.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;

    /// <summary>Show or hide the text-input section.</summary>
    public void ShowTextInputSection(bool visible)
        => inputContainer.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;

    public void ShowCompletionSection(bool visible)
    => taskCompletionContainer.style.display = visible
        ? DisplayStyle.Flex
        : DisplayStyle.None;
>>>>>>> Stashed changes

    public void SetTaskCompletionContainer(
        string completionTitleFallback,
        string completionQuoteFallback)
    {
        completionTitleLabel.text = string.IsNullOrWhiteSpace(completionTitleFallback)
            ? "TASK COMPLETE"
            : completionTitleFallback;

        completionQuoteLabel.text = string.IsNullOrWhiteSpace(completionQuoteFallback)
            ? "Truth flows onward."
            : completionQuoteFallback;
    }

<<<<<<< Updated upstream
=======
    public void SetTaskHeader(
        string missionName,
        string taskTitleOrFallback,
        string instructionOrFallback,
        int currentStep,
        int totalSteps)
    {
        // Mission name
        missionTitleLabel.text = missionName;

        // Task title
        taskTitleLabel.text = string.IsNullOrWhiteSpace(taskTitleOrFallback)
            ? "Untitled Task"
            : taskTitleOrFallback;

        // Instruction
        instructionLabel.text = string.IsNullOrWhiteSpace(instructionOrFallback)
            ? "No instruction."
            : instructionOrFallback;

        // Step counter
        stepLabel.text = $"{currentStep}/{totalSteps}";
    }
>>>>>>> Stashed changes

    /// <summary>
    /// Called by MissionManager.OnTaskChanged to update header and route logic.
    /// </summary>
    private void UpdateTaskUI(MissionTask task, int idx, int total)
    {
        // 1) Reset visibility: header always on, question & input off
        ShowHeader(true);
<<<<<<< Updated upstream
        // ShowQuestionSection(false);
        // ShowTextInputSection(false);
=======
        ShowQuestionSection(false);
        ShowTextInputSection(false);
>>>>>>> Stashed changes

        // 2) Pull out first line of the task title (fallback will be handled in SetTaskHeader)
        string taskTitle = !string.IsNullOrWhiteSpace(task.title)
            ? task.title.Split('\n')[0]
            : "";

        // 3) Call SetTaskHeader to populate mission name, (possibly blank) title, full instruction, and “step/total”
        SetTaskHeader(
            missionName: MissionManager.Instance.MissionAsset.missionName,
            taskTitleOrFallback: taskTitle,
            instructionOrFallback: task.instruction,
            currentStep: idx + 1,
            totalSteps: total
            );
    }

    /// <summary>
    /// Shows a question with 2 choices and listens for a callback.
    /// </summary>

    // We need these as fields so we can unregister them later
    private EventCallback<PointerDownEvent> _OnChoiceAPointerDown;
    private EventCallback<PointerUpEvent> _OnChoiceAPointerUp;
    private EventCallback<PointerDownEvent> _OnChoiceBPointerDown;
    private EventCallback<PointerUpEvent> _OnChoiceBPointerUp;

    public void ConfigureQuestionUI(
        string questionText,
        string choiceAText,
        string choiceBText,
        int correctChoiceIndex,
        Action<bool> onAnswered)
    {
        // … (null checks + hide error + populate labels + show container) …
        if (questionContainer == null || questionTextLabel == null ||
           choiceAButton == null || choiceBButton == null ||
           choiceALabel == null || choiceBLabel == null ||
           choiceErrorLabel == null)
        {
            Debug.LogError("[UIController] Missing Question UI elements.");
            return;
        }
        choiceErrorLabel.style.display = DisplayStyle.None;

        // 2) Assign the question text and the two choices
        questionTextLabel.text = questionText ?? "No question provided.";
        choiceALabel.text = choiceAText ?? "A";
        choiceBLabel.text = choiceBText ?? "B";

        // 3) Save the correctChoiceIndex & onAnswered callback in local fields if needed
        this.correctChoiceIndex = correctChoiceIndex;
<<<<<<< Updated upstream
        this.onQuestionAnswered = _ => onAnswered((bool)_);

=======
        this.onQuestionAnswered?.Invoke(onAnswered);
>>>>>>> Stashed changes

        // 4) Wipe out any old handlers & reset state:
        if (_OnChoiceAPointerDown != null)
        {
            choiceAButton.UnregisterCallback<PointerDownEvent>(_OnChoiceAPointerDown);
            _OnChoiceAPointerDown = null;
        }
        if (_OnChoiceAPointerUp != null)
        {
            choiceAButton.UnregisterCallback<PointerUpEvent>(_OnChoiceAPointerUp);
            _OnChoiceAPointerUp = null;
        }
        if (_OnChoiceBPointerDown != null)
        {
            choiceBButton.UnregisterCallback<PointerDownEvent>(_OnChoiceBPointerDown);
            _OnChoiceBPointerDown = null;
        }
        if (_OnChoiceBPointerUp != null)
        {
            choiceBButton.UnregisterCallback<PointerUpEvent>(_OnChoiceBPointerUp);
            _OnChoiceBPointerUp = null;
        }
        choiceAPress.isPressed = false;
        choiceBPress.isPressed = false;

        // 5) Build the “handle answer” closures:
        void HandleAnswerA() => onAnswered(correctChoiceIndex == 0);
        void HandleAnswerB() => onAnswered(correctChoiceIndex == 1);

        // 6) Create new delegates for Choice A:
        _OnChoiceAPointerDown = evt =>
        {
            Debug.Log("[UI] *** Choice A Got PointerDown ***");
            choiceAPress.isPressed = true;
            choiceAPress.pressStartTime = Time.time;
        };

        _OnChoiceAPointerUp = evt =>
        {
            Debug.Log("[UI] *** Choice A Got PointerUp at time " + Time.time +
                      "   (was isPressed: " + choiceAPress.isPressed + ")");
            if (!choiceAPress.isPressed)
            {
                Debug.Log("[UI]   → Up but isPressed was FALSE, so ignoring.");
                return;
            }

            choiceAPress.isPressed = false;
            float elapsed = Time.time - (float)choiceAPress.pressStartTime;
            Debug.Log($"[UI]   → Choice A held {elapsed:F3}s (threshold={HOLD_THRESHOLD})");
            if (elapsed >= HOLD_THRESHOLD)
            {
                Debug.Log("[UI]   → met threshold → HandleAnswerA()");
                HandleAnswerA();
            }
            else
            {
                Debug.Log("[UI]   → released too soon; skipping.");
            }
        };

        // 7) Create new delegates for Choice B:
        _OnChoiceBPointerDown = evt =>
        {
            Debug.Log("[UI] *** Choice B Got PointerDown ***");
            choiceBPress.isPressed = true;
            choiceBPress.pressStartTime = Time.time;
        };

        _OnChoiceBPointerUp = evt =>
        {
            Debug.Log("[UI] *** Choice B Got PointerUp at time " + Time.time +
                      "   (was isPressed: " + choiceBPress.isPressed + ")");
            if (!choiceBPress.isPressed)
            {
                Debug.Log("[UI]   → Up but isPressed was FALSE, so ignoring.");
                return;
            }

            choiceBPress.isPressed = false;
            float elapsed = Time.time - (float)choiceBPress.pressStartTime;
            Debug.Log($"[UI]   → Choice B held {elapsed:F3}s (threshold={HOLD_THRESHOLD})");
            if (elapsed >= HOLD_THRESHOLD)
            {
                Debug.Log("[UI]   → met threshold → HandleAnswerB()");
                HandleAnswerB();
            }
            else
            {
                Debug.Log("[UI]   → released too soon; skipping.");
            }
        };

        // 8) Register the new callbacks—BUT use TrickleDown for PointerDown:
        if (choiceAButton == null)
        {
            Debug.LogError("[UI] choiceAButton is null; cannot register ChoiceA callbacks.");
        }
        else
        {
            Debug.Log("[UI] Registering _OnChoiceAPointerDown (TrickleDown) on choiceAButton = " + choiceAButton);
            choiceAButton.RegisterCallback<PointerDownEvent>(
                _OnChoiceAPointerDown,
                TrickleDown.TrickleDown // <--- must listen in trickle phase
            );

            Debug.Log("[UI] Registering _OnChoiceAPointerUp (bubble) on choiceAButton = " + choiceAButton);
            choiceAButton.RegisterCallback<PointerUpEvent>(_OnChoiceAPointerUp);
        }

        if (choiceBButton == null)
        {
            Debug.LogError("[UI] choiceBButton is null; cannot register ChoiceB callbacks.");
        }
        else
        {
            Debug.Log("[UI] Registering _OnChoiceBPointerDown (TrickleDown) on choiceBButton = " + choiceBButton);
            choiceBButton.RegisterCallback<PointerDownEvent>(
                _OnChoiceBPointerDown,
                TrickleDown.TrickleDown // <--- listen early
            );

            Debug.Log("[UI] Registering _OnChoiceBPointerUp (bubble) on choiceBButton = " + choiceBButton);
            choiceBButton.RegisterCallback<PointerUpEvent>(_OnChoiceBPointerUp);
        }
    }

<<<<<<< Updated upstream
    public void ConfigureQuestionUIFlexible(
     string questionText,
     IList<string> choices,
     int correctChoiceIndex,
     Action<bool> onAnswered)
    {
        if (questionContainer == null || questionTextLabel == null || choicesHost == null)
        {
            Debug.LogError("[UIController] Missing Question UI elements for flexible setup.");
            return;
        }

        // Hide legacy error text and legacy A/B widgets
        if (choiceErrorLabel != null) choiceErrorLabel.style.display = DisplayStyle.None;
        if (choiceAButton != null) choiceAButton.style.display = DisplayStyle.None;
        if (choiceBButton != null) choiceBButton.style.display = DisplayStyle.None;
        if (choiceALabel != null) choiceALabel.style.display = DisplayStyle.None;
        if (choiceBLabel != null) choiceBLabel.style.display = DisplayStyle.None;

        // Header text
        questionTextLabel.text = string.IsNullOrWhiteSpace(questionText) ? "No question provided." : questionText;

        // Full reset of dynamic area
        ResetTaskUI();

        // Guard
        if (choices == null || choices.Count < 2)
        {
            Debug.LogWarning("[UIController] Choices < 2; falling back to simple AB.");
            ShowQuestionSection(true);
            ConfigureQuestionUI(
                questionText,
                choices != null && choices.Count > 0 ? choices[0] : "A",
                choices != null && choices.Count > 1 ? choices[1] : "B",
                correctChoiceIndex,
                onAnswered
            );
            return;
        }

        // Build N dynamic buttons
        for (int i = 0; i < choices.Count; i++)
        {
            int idx = i; // capture
            var btn = new Button { name = $"choiceButton_{idx}" };
            btn.AddToClassList("choice-button");
            if (idx > 0) btn.style.marginTop = 6;
            btn.text = string.IsNullOrWhiteSpace(choices[idx]) ? $"Option {idx + 1}" : choices[idx];

            // Ensure internal label (if present) doesn't steal events
            var lbl = btn.Q<Label>();
            if (lbl != null) lbl.pickingMode = PickingMode.Ignore;

            // Track press state for hold-to-confirm UX
            _pressStates[btn] = (false, 0);

            // Handlers we can later unregister
            EventCallback<PointerDownEvent> onDown = evt =>
            {
                var st = _pressStates[btn];
                st.pressed = true;
                st.t0 = Time.time;
                _pressStates[btn] = st;
            };

            EventCallback<PointerUpEvent> onUp = evt =>
            {
                var st = _pressStates[btn];
                if (!st.pressed) return;
                st.pressed = false;
                _pressStates[btn] = st;

                float held = Time.time - (float)st.t0;
                if (held >= HOLD_THRESHOLD)
                {
                    bool isCorrect = (idx == correctChoiceIndex);
                    onAnswered?.Invoke(isCorrect);
                }
            };

            // Register (PointerDown in trickle phase, Up in bubble)
            btn.RegisterCallback(onDown, TrickleDown.TrickleDown);
            btn.RegisterCallback(onUp);

            // Keep references for safe unregistration
            _dynHandlers[btn] = (onDown, onUp);

            // Mount
            _dynamicChoiceButtons.Add(btn);
            choicesHost.Add(btn);
        }

        // Show section once ready
        ShowQuestionSection(true);
    }
=======


>>>>>>> Stashed changes

    // Example ShowChoiceError remains unchanged
    public void ShowChoiceError(string message)
    {
        if (choiceErrorLabel != null)
        {
            choiceErrorLabel.text = message;
            choiceErrorLabel.style.display = DisplayStyle.Flex;
        }
    }


    public void SetChoiceAText(string newText)
    {
        if (choiceALabel != null)
            choiceALabel.text = newText;
    }
    public void SetChoiceBText(string newText)
    {
        if (choiceBButton != null)
            choiceBButton.text = newText;
    }

    /// <summary>
    /// Configure and display the text-input UI.
    /// </summary>
    public void ConfigureTextInputUI(string instructionInputText, string inputPlaceholder, System.Action<string> onSubmit)
    {
        if (inputContainer == null ||
            answerPlaceField == null ||
            inputField == null ||
            submitButton == null ||
            inputErrorLabel == null)
        {
            Debug.LogError("[UIController] Missing Input UI elements.");
            return;
        }

        // 1) Hide any prior error
        inputErrorLabel.style.display = DisplayStyle.None;

        // 2) Set placeholder text and clear the field
        answerPlaceField.text = instructionInputText;
        inputField.value = inputPlaceholder;


        // 3) Remove old callback (if any)
        //    Always guard against null before UnregisterCallback

        submitButton.clicked -= HandleSubmit;

        if (_OnSubmitPointerDown != null)
        {
            submitButton.UnregisterCallback<PointerDownEvent>(_OnSubmitPointerDown);
            _OnSubmitPointerDown = null;
        }

        if (_OnSubmitPointerUp != null)
        {
            submitButton.UnregisterCallback<PointerUpEvent>(_OnSubmitPointerUp);
            _OnSubmitPointerUp = null;
        }

        submitPress.isPressed = false;


        // 4) Ensure Submit’s label doesn’t intercept clicks:
        submitButton.Q<Label>().pickingMode = PickingMode.Ignore;

        // 5) Build the new pointer‐down/up delegates:
        _OnSubmitPointerDown = evt =>
        {
            submitPress.isPressed = true;
            submitPress.pressStartTime = Time.time;
            Debug.Log($"[UI] *** Submit Got PointerDown at {Time.time:F3} ***");
        };
        _OnSubmitPointerUp = evt =>
        {
            Debug.Log($"[UI] *** Submit Got PointerUp at {Time.time:F3} (wasPressed={submitPress.isPressed}) ***");
            if (!submitPress.isPressed)
            {
                Debug.Log("[UI]   → Up but isPressed was FALSE, so ignoring.");
                return;
            }
            submitPress.isPressed = false;

            float elapsed = Time.time - (float)submitPress.pressStartTime;
            Debug.Log($"[UI]   → Submit held {elapsed:F3}s (threshold={HOLD_THRESHOLD})");
            if (elapsed >= HOLD_THRESHOLD)
            {
                Debug.Log("[UI]   → met threshold → invoking onSubmit()");
                onSubmit(inputField.value);
            }
            else
            {
                Debug.Log("[UI]   → released too soon; skipping.");
            }
        };

        // 6) Register the new callbacks—PointerDown must be TrickleDown, Up can bubble:
        submitButton.RegisterCallback<PointerDownEvent>(
            _OnSubmitPointerDown,
            TrickleDown.TrickleDown   // ← critical: catch before Button eats it
        );
        submitButton.RegisterCallback<PointerUpEvent>(_OnSubmitPointerUp);

        // 7) Finally, show the input container
        inputContainer.style.display = DisplayStyle.Flex;
    }

    private void HandleSubmit()
    {
        Debug.Log("[MissionTaskUI] Submit clicked: " + inputField.value);
        onTextSubmitted?.Invoke(inputField.value);
    }

    public void ShowInputError(string message)
    {
        if (inputErrorLabel != null)
        {
            inputErrorLabel.text = message;
            inputErrorLabel.style.display = DisplayStyle.Flex;
        }
    }

    // Example: show how to disable the button frame but keep label styling
    public void DisableChoiceAButton()
    {
        if (choiceAButton != null)
            choiceAButton.SetEnabled(false);
        // Because we styled .choice-button:disabled and .choice-button-text appropriately,
        // it will appear “grayed out” automatically.
    }
<<<<<<< Updated upstream

    /// <summary>
    /// Resets task UI (labels + dynamic choices).
    /// </summary>
    public void ResetTaskUI()
    {
        // === Reset text fields ===
        missionTitleLabel.text = string.Empty;
        taskTitleLabel.text = string.Empty;
        stepLabel.text = string.Empty;
        instructionLabel.text = string.Empty;

        // === Clear dynamic choices ===
        foreach (var btn in _dynamicChoiceButtons)
        {
            // Unregister any pointer callbacks we attached earlier
            btn.UnregisterCallback<PointerDownEvent>((EventCallback<PointerDownEvent>)null);
            btn.UnregisterCallback<PointerUpEvent>((EventCallback<PointerUpEvent>)null);

            // Remove from visual hierarchy
            btn.RemoveFromHierarchy();
        }

        _dynamicChoiceButtons.Clear();
        _pressStates.Clear();

        if (choicesHost != null)
            choicesHost.Clear();
    }

    private void TrySubscribeOrRetry()
    {
        // If MissionManager is already alive, subscribe now.
        var mgr = MissionManager.Instance;
        if (mgr != null)
        {
            showHeaderOnStart = () => ShowHeader(true);
            hideHeaderOnComplete = () => ShowHeader(false);

            mgr.OnMissionStarted += showHeaderOnStart;
            mgr.OnTaskChanged += UpdateTaskUI;
            mgr.OnMissionComplete += hideHeaderOnComplete;

            // If we’re late to the party, render the current state immediately.
            if (mgr.HasActiveMission) // add a public bool in MissionManager if you don't have it
            {
                ShowHeader(true);
                var (task, idx, total) = mgr.GetCurrentTaskSnapshot(); // add a tiny getter
                if (task != null) UpdateTaskUI(task, idx, total);
            }
            return;
        }

        // Otherwise retry until MissionManager comes alive.
        if (_subscribeCo == null) _subscribeCo = StartCoroutine(WaitAndSubscribe());
    }

    private System.Collections.IEnumerator WaitAndSubscribe()
    {
        // Retry a few times per second until Instance appears.
        while (MissionManager.Instance == null) yield return null;
        _subscribeCo = null;
        TrySubscribeOrRetry();
    }

=======
>>>>>>> Stashed changes
    public void HideAllTaskUI()
    {
        if (questionContainer != null) questionContainer.style.display = DisplayStyle.None;
        if (inputContainer != null) inputContainer.style.display = DisplayStyle.None;
        if (taskCompletionContainer != null) inputContainer.style.display = DisplayStyle.None;
    }
}

