using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

///<summary>
/// Manages dialog playback for both timeline-driven (standalone) and injected (mission/POI) flows.
/// Provides two explicit entry points with verbose logging:
///  • StartStandaloneDialogue() ➔ advances timeline on completion
///  • StartInjectedDialogue() ➔ returns control to mission logic on completion
///</summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Controller")]
    [SerializeField] private DialogUIController uiController;

    [Header("Typewriter Settings")]
    [SerializeField, Min(0f)] private float typewriterSpeed = 0.02f;

    [Header("UI Timings")]
    [Tooltip("How long to wait after text finishes before showing the Next button.")]
    [SerializeField] private float nextButtonDelay = 0.5f;

    [Header("Global Effects (for testing)")]
    [SerializeField] private DistortionControllerUITK distortion;

   [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private Queue<DialogLine> lines;
    private bool isWaitingForNext;
    private bool _waitingForVoiceOver;
    private AudioClip _currentVoiceOver;

    // ─────────────────────────────────────────────────────────────────────────
    // INTERNAL "UI-complete" event, fired when ProcessDialogue finishes showing all lines
    // and the internal overlay Close button is clicked.
    // ─────────────────────────────────────────────────────────────────────────
    [SerializeField] private UnityEvent onDialogueIntComplete = new UnityEvent();
    public UnityEvent OnDialogueIntComplete => onDialogueIntComplete;

    // ─────────────────────────────────────────────────────────────────────────
    // EXTERNAL "dialogue fully complete" events: exactly one will fire.
    //  • onDialogueTimelineComplete: for standalone timeline dialogs
    //  • onDialogueMissionComplete: for injected mission/POI dialogs
    // ─────────────────────────────────────────────────────────────────────────
    [Header("External Completion Events")]
    [SerializeField] private UnityEvent onDialogueTimelineComplete = new UnityEvent();
    [SerializeField] private UnityEvent onDialogueMissionComplete = new UnityEvent();
    public UnityEvent OnDialogueTimelineComplete => onDialogueTimelineComplete;
    public UnityEvent OnDialogueMissionComplete => onDialogueMissionComplete;

    // Tracks which “finish” event to fire at the end of ProcessDialogue
    private enum EndMode { None, Timeline, Mission }
    private EndMode _endMode = EndMode.None;

    private bool _hasRuntimeListener = false;

    private DialogAsset _currentDialogAsset;

    private void Awake()
    {
        var docs = FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[Sanity] UIDocuments in scene: {docs.Length}");

        Debug.Log("[DialogueManager] Awake: Initializing instance");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DialogueManager] Duplicate instance, destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure UnityEvents are non-null
        if (onDialogueIntComplete == null) onDialogueIntComplete = new UnityEvent();
        if (onDialogueTimelineComplete == null) onDialogueTimelineComplete = new UnityEvent();
        if (onDialogueMissionComplete == null) onDialogueMissionComplete = new UnityEvent();

        // ─── UI hookup ───────────────────────────────
        if (uiController == null)
            uiController = FindFirstObjectByType<DialogUIController>();
        uiController.nextButton.clicked += OnNextClicked;

        // ─── Distortion hookup ───────────────────────
        if (distortion == null)
            distortion = FindFirstObjectByType<DistortionControllerUITK>();
        
        // ─── Audio hookup ────────────────────────────
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();


        if (debugLogging)
        {
            Debug.Log($"[DialogueManager] uiController     = {(uiController != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogueManager] nextButton       = {(uiController?.nextButton != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogueManager] distortion       = {(distortion != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogueManager] audioSource      = {(audioSource != null ? "OK" : "NULL")}"); 
        
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[DialogueManager] OnDestroy: Cleaning up listeners");
        if (uiController != null)
            uiController.nextButton.clicked -= OnNextClicked;
    }


    private Coroutine currentDialogueCoroutine;

    /// <summary>
    /// Starts a timeline‐driven dialog block.
    /// </summary>
    public void StartStandaloneDialogue(DialogAsset dialogAsset)
    {
        // Stop only the old dialogue coroutine
        if (currentDialogueCoroutine != null)
            StopCoroutine(currentDialogueCoroutine);

        // 1) Enqueue the lines
        lines = new Queue<DialogLine>(dialogAsset.lines);

        _currentDialogAsset = dialogAsset;

        // 2) Set mode & show the UI
        _endMode = EndMode.Timeline;
        uiController.ShowOverlay();

        Debug.Log($"[DialogueManager] StartStandaloneDialogue('{dialogAsset.name}') with {lines.Count} lines");

        // 2a) Send trigger event
        TriggerSenderHelpers.FireDialogTrigger(_currentDialogAsset.assetID, TriggerSubEventType.DialogStarted);

        // 3) Launch the process
        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
    }

    /// <summary>
    /// Starts an injected dialog (e.g. a POI narrative) that on completion calls back into MissionManager.
    /// </summary>
    public void StartInjectedDialogue(DialogAsset dialogAsset, UnityAction onComplete)
    {
        if (currentDialogueCoroutine != null)
            StopCoroutine(currentDialogueCoroutine);

        lines = new Queue<DialogLine>(dialogAsset.lines);
        _currentDialogAsset = dialogAsset;

        _endMode = EndMode.Mission;
        onDialogueMissionComplete.RemoveAllListeners();
        onDialogueMissionComplete.AddListener(onComplete);

        uiController.ShowOverlay();

        Debug.Log($"[DialogueManager] StartInjectedDialogue('{dialogAsset.name}') with {lines.Count} lines");

        TriggerSenderHelpers.FireDialogTrigger(_currentDialogAsset.assetID, TriggerSubEventType.DialogStarted);

        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
    }


    /// <summary>
    /// Original dialog start for raw lines + optional voice-over.
    /// Subscribes to internal UI-complete event to then wait for audio/overlay close.
    /// </summary>
    public void StartDialogue(DialogAsset asset)
    {
        StartStandaloneDialogue(asset);
    }

    // In your class:
    private Action _finalClickedHandler;

    private IEnumerator ProcessDialogue()
    {
        var captureMode = _endMode;
        Debug.Log("[DialogueManager] ProcessDialogue: Beginning coroutine");
        while (lines.Count > 0)
        {
            var line = lines.Dequeue();
            Debug.Log($"[DialogueManager] Processing line: Speaker='{line.speakerName}', Text='{line.text}'");

            uiController.nextButton.style.display = DisplayStyle.None;
            uiController.choiceA.style.display = DisplayStyle.None;
            uiController.choiceB.style.display = DisplayStyle.None;

            if (line.triggerGlitch)
            {
                Debug.Log($"[DialogueManager] Triggering glitch pattern {line.glitchPattern} for {line.glitchDuration}s");
                distortion.PlayGlitch(line.glitchPattern);
                yield return new WaitForSeconds(line.glitchDuration);
                distortion.StopGlitch();
                Debug.Log("[DialogueManager] Glitch ended");
            }

            uiController.speakerLabel.text = line.speakerName;
            yield return StartCoroutine(TypeText(line.text));

            if (line.choices != null && line.choices.Length == 2)
            {
                Debug.Log("[DialogueManager] Presenting choices to player");
                yield return StartCoroutine(HandleChoiceLine(line));
                yield break;
            }
            else
            {
                if (lines.Count > 0)
                {
                    Debug.Log("[DialogueManager] Showing Next (lines left = " + lines.Count + ")");
                    yield return StartCoroutine(ShowNextButtonWithDelay());
                    isWaitingForNext = true;
                    yield return new WaitUntil(() => !isWaitingForNext);
                    Debug.Log("[DialogueManager] Next clicked → continuing");
                }
                // 3) Otherwise → “Final” (either Close or Next-to-timeline)
                else
                {
                    Debug.Log("[DialogueManager] Last line → wiring final");
                    // pass the captured end-mode into the final-button helper:
                    yield return StartCoroutine(ShowFinalButtonWithDelay(captureMode));
                    yield break;
                }
            }
        }
    }

    private IEnumerator ShowFinalButtonWithDelay(EndMode captureMode)
    {
        yield return new WaitForSeconds(nextButtonDelay);

        var btn = uiController.nextButton;
        btn.BringToFront();
        btn.pickingMode = PickingMode.Position;
        btn.style.display = DisplayStyle.Flex;

        // Determine button text based on the captured mode
        bool isTimeline = (captureMode == EndMode.Timeline);
        btn.text = isTimeline ? "Next" : "Close";  // <-- exact spot to insert the bool

        // Tear down old listeners
        btn.clicked -= OnNextClicked;
        if (_finalClickedHandler != null)
            btn.clicked -= _finalClickedHandler;

        // Wire final handler once
        _finalClickedHandler = () =>
        {
            Debug.Log("[DialogueManager] Final button clicked → closing (mode=" + captureMode + ")");
            uiController.HideOverlay();
            if (captureMode == EndMode.Timeline)
                onDialogueTimelineComplete.Invoke();
            else
                onDialogueMissionComplete.Invoke();

            // Reset and unregister
            _endMode = EndMode.None;
            btn.clicked -= _finalClickedHandler;
        };
        btn.clicked += _finalClickedHandler;
        TriggerSenderHelpers.FireDialogTrigger(_currentDialogAsset.assetID, TriggerSubEventType.DialogCompleted);
    }



    private IEnumerator ShowNextButtonWithDelay()
    {
        yield return new WaitForSeconds(nextButtonDelay);
        uiController.nextButton.style.display = DisplayStyle.Flex;
        Debug.Log("[DialogueManager] NextButton.pickingMode = " + uiController.nextButton.pickingMode);
        var btn = uiController.nextButton;
        btn.clicked -= OnNextClicked;  // clear any old wiring
        btn.clicked += OnNextClicked;  // re-attach the “advance line” handler

        uiController.nextButton.BringToFront();
        uiController.nextButton.pickingMode = PickingMode.Position;
        uiController.nextButton.focusable = true;
        uiController.nextButton.SetEnabled(true);
    }

    private void OnNextClicked()
    {
        Debug.Log("[DialogueManager] OnNextClicked handler fired");
        isWaitingForNext = false;
    }

    private IEnumerator TypeText(string fullText)
    {
        uiController.bodyLabel.text = string.Empty;
        _waitingForVoiceOver = false;
        _currentVoiceOver = null;
        foreach (var queued in lines)
            if (queued.text == fullText)
            {
                _currentVoiceOver = queued.voiceOver;
                break;
            }

        foreach (char c in fullText)
        {
            uiController.bodyLabel.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        if (_currentVoiceOver != null)
            StartCoroutine(PlayVoiceOver(_currentVoiceOver));
    }

    private IEnumerator HandleChoiceLine(DialogLine line)
    {
        int selected = -1;
        uiController.choiceA.style.display = DisplayStyle.Flex;
        uiController.choiceB.style.display = DisplayStyle.Flex;
        uiController.nextButton.style.display = DisplayStyle.None;
        uiController.choiceALabel.text = line.choices[0];
        uiController.choiceBLabel.text = line.choices[1];

        Action onA = () => selected = 0;
        Action onB = () => selected = 1;
        uiController.choiceA.clicked += onA;
        uiController.choiceB.clicked += onB;

        Debug.Log("[DialogueManager] Waiting for player choice...");
        yield return new WaitUntil(() => selected != -1);

        uiController.choiceA.clicked -= onA;
        uiController.choiceB.clicked -= onB;
        uiController.choiceA.style.display = DisplayStyle.None;
        uiController.choiceB.style.display = DisplayStyle.None;
        uiController.nextButton.style.display = DisplayStyle.Flex;

        var nextAsset = selected == 0 ? line.nextOnChoice0 : line.nextOnChoice1;
        if (nextAsset != null)
        {
            Debug.Log($"[DialogueManager] Player chose option {selected}");
            StartDialogue(nextAsset);
            yield break;
        }

        isWaitingForNext = true;
        yield return new WaitUntil(() => !isWaitingForNext);
    }

    private void OnUICompleted_PlayAudioThenFinish()
    {
        onDialogueIntComplete.RemoveListener(OnUICompleted_PlayAudioThenFinish);
        Debug.Log("[DialogueManager] Unregistering RuntimeTimelineListener");
        UnregisterRuntimeTimelineListener();
        Debug.Log("[DialogueManager] ClosingDialogue");
        StartCoroutine(WaitForAudioAndUIClose());
    }

    private IEnumerator WaitForAudioAndUIClose()
    {
        while (!_waitingForVoiceOver)
            yield return null;
        while (uiController.overlay.style.display == DisplayStyle.Flex)
            yield return null;

        bool timeline = HasTimelineListener();
        Debug.Log($"[DialogueManager] FinishInternal: Hiding overlay and invoking " +
                  (timeline ? "TimelineComplete" : "MissionComplete"));
        uiController.overlay.style.display = DisplayStyle.None;
        if (timeline)
            onDialogueTimelineComplete.Invoke();
        else
            onDialogueMissionComplete.Invoke();
    }

    private IEnumerator PlayVoiceOver(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.Log("[DialogueManager] No voice-over clip, marking complete");
            _waitingForVoiceOver = true;
            yield break;
        }
      /*  audioSource.clip = clip;
        audioSource.Play();
        while (audioSource.isPlaying)
            yield return null;
        _waitingForVoiceOver = true;*/
    }

    public bool HasTimelineListener()
    {
        return _hasRuntimeListener ||
               (onDialogueTimelineComplete != null && onDialogueTimelineComplete.GetPersistentEventCount() > 0);
    }

    public void RegisterRuntimeTimelineListener()
    {
        Debug.Log("[DialogueManager] RegisterRuntimeTimelineListener called");
        _hasRuntimeListener = true;
    }

    public void UnregisterRuntimeTimelineListener()
    {
        Debug.Log("[DialogueManager] UnregisterRuntimeTimelineListener called");
        _hasRuntimeListener = false;
    }
}
