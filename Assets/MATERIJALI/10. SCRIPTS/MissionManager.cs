using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using MissionTaskType = MissionTask.MissionTaskType;

/// <summary>
/// Controls overall mission flow: GPS waypoints, POI interactions, question and text-input tasks.
/// Integrates with DialogueManager for injected narrative playback.
/// </summary>
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] private ScoreUIController scoreUI;

    [SerializeField, Tooltip("Isključi bodovanje iz ovog kontrolera")]
    private bool enableScoringInThisMission = true;
    int pts;
    int bonus;
    int pen;

    [Header("Mission Data (assign in Inspector)")]
    [Tooltip("The ScriptableObject defining this mission’s tasks")]
    [SerializeField] private MissionSO missionAsset;
    public MissionSO MissionAsset => missionAsset;

    [Header("UI Controllers")]
    [SerializeField] private MissionTaskUIController taskUI;
    [SerializeField] private MissionGPSHUDUIController gpsUI;
    [SerializeField] private POIInteractionDetector poiDetector;
    [SerializeField] private MapOverlayController mapOverlayController;


    [Header("Managers")]
    [SerializeField] private WaypointManager waypointManager;

    [Header("Settings")]
    [SerializeField, Tooltip("Time (s) inside a geofence before confirming")] 
    private float gpsConfirmationTime = 3f;
    [SerializeField, Tooltip("Max distance (m) to allow POI interaction")] 
    private float poiInteractionDistance = 50f;

    // Mission lifecycle events
    public event Action OnMissionStarted;
    public event Action<MissionTask, int, int> OnTaskChanged;
    public event Action OnMissionComplete;

    // Internal state
    private List<MissionTask> _tasks;
    private int _currentIndex = -1;
    private string _activeWaypointId;
    private bool _taskCompleted;
    private WaypointData _completedWaypointTarget;


    // GPS provider
    [Header("GPS Provider")]
    [SerializeField] private MonoBehaviour gpsProviderSource;
    private IGpsProvider _gps => gpsProviderSource as IGpsProvider;
    public IGpsProvider GpsProvider => _gps;
    public float CurrentTaskTolerance => gpsConfirmationTime;

    // Handlers for cleanup
    private Action<string, WaypointZoneLevel> gpsWaypointHandler;
    private Action<POIExtendedController> poiConfirmedHandler;

    private HashSet<string> completedWaypointIds = new HashSet<string>();

    public UnityEvent<MissionSO> OnMissionLoaded = new UnityEvent<MissionSO>();
    public bool IsInitialized { get; private set; }
    public MissionSO CurrentMission { get; private set; }  // expose if not already

    private void Awake()
    {
        Debug.Log("[MissionManager] Awake: initializing singleton");
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (_gps == null)
            Debug.LogError("[MissionManager] No IGpsProvider assigned — check Inspector.");
        else
            Debug.Log($"[MissionManager] GPS provider → {_gps.GetType().Name}");

        if (missionAsset == null)
            Debug.LogWarning("[MissionManager] No default MissionSO assigned; tasks may come from Timeline blocks.");
        if (taskUI == null) Debug.LogError("[MissionManager] TaskUIController not set!");
        if (waypointManager == null) Debug.LogError("[MissionManager] WaypointManager not set!");
        if (poiDetector == null) Debug.LogError("[MissionManager] POIInteractionDetector not set!");

        // Poveži globalno bodovanje sa ovim flagom
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScoringEnabled(enableScoringInThisMission);
        }

        if (ScoreManager.Instance != null)
        {
            pts = ScoreManager.Instance.PointsPerTask;
            bonus = ScoreManager.Instance.PointsOnMissionComplete;
            pen = ScoreManager.Instance.PenaltyForWrongAnswer;
        }
        else
        {
            Debug.LogWarning("[MissionManager] ScoreManager.Instance je null u Awake!");
            pts = bonus = pen = 0;
        }


    }

  

    private void OnEnable()
    {
        MissionBlockSO.OnMissionBlockAboutToFire += HandleMissionAnnouncement;
    }

    private void OnDestroy()
    {
        Debug.Log("[MissionManager] OnDestroy: unsubscribing handlers");
        waypointManager.OnWaypointZoneEntered -= gpsWaypointHandler;
        MissionBlockSO.OnMissionBlockAboutToFire -= HandleMissionAnnouncement;
    }

    private void HandleMissionAnnouncement(MissionSO announced)
    {
        if (announced == null)
        {
            Debug.LogWarning("[MissionManager] Null mission announcement—ignoring.");
            return;
        }
        missionAsset = announced;
        Debug.Log($"[MissionManager] Mission asset set via announcement: {missionAsset.missionName}");
        var totalSteps = missionAsset?.tasks != null ? missionAsset.tasks.Count : 0;
        taskUI?.SetTaskHeader(missionAsset.missionName, string.Empty, string.Empty, 0, totalSteps);
    }

    /// <summary>
    /// Called by POIExtendedController when the user interacts with a POI.
    /// Finds the corresponding POI controller and injects its dialogue.
    /// </summary>
    [Obsolete]
    public void OnPOIInteraction(string poiId)
    {
        Debug.Log($"[MissionManager] OnPOIInteraction('{poiId}')");
        var poiExt = FindObjectsOfType<POIExtendedController>()
            .FirstOrDefault(c => c.definition != null && c.definition.id == poiId);
        if (poiExt != null)
        {
            Debug.Log($"[MissionManager] Found POIExtendedController for '{poiId}' → injecting dialogue");
            HandlePoiInteraction(poiExt);
        }
        else
        {
            Debug.LogWarning($"[MissionManager] Could not find POI controller for '{poiId}'");
        }
    }

    public void BeginMission(MissionSO customAsset = null)
    {
        missionAsset = customAsset ?? missionAsset;
        if (missionAsset == null)
        {
            Debug.LogError("[MissionManager] No mission asset provided! Aborting.");
            return;
        }

        Debug.Log($"[MissionManager] BeginMission: {missionAsset.missionName}");
        _tasks = missionAsset.tasks;
        // Never reuse serialized completion flags left by editor/test runs.
        foreach (var missionTask in _tasks) missionTask.isCompleted = false;
        _currentIndex = -1;

        taskUI.ShowHeader(true);
        taskUI.ShowCompletionSection(false);
        taskUI.ShowQuestionSection(false);
        taskUI.ShowTextInputSection(false);
        OnMissionStarted?.Invoke();
        Debug.Log("[MissionManager] OnMissionStarted fired");

        AdvanceToNextTask();
    }


    private void AdvanceToNextTask()
    {
        _taskCompleted = false;
        CleanupPreviousTask();
        _currentIndex++;
        Debug.Log($"[MissionManager] Advancing to task index {_currentIndex}");

        if (_currentIndex >= _tasks.Count)
        {
            if (enableScoringInThisMission)
            {
                ScoreManager.Instance.AddMissionCompletePoints();
                Debug.Log($"[MissionManager] +{bonus} points for mission complete");
            }

            Debug.Log("[MissionManager] All tasks complete — finishing mission");

            taskUI.ShowHeader(false);

            if (_completedWaypointTarget != null && !string.IsNullOrEmpty(_completedWaypointTarget.id))
            {
                MarkWaypointAsCompleted(_completedWaypointTarget.id);
                mapOverlayController?.RefreshWaypointStates();
            }

            // Release GPS HUD and clear active target
            gpsUI?.ClearActiveTarget();
            gpsUI?.ClearMissionMode();
            _activeWaypointId = null;
            _completedWaypointTarget = null;
            missionAsset = null;

            try
            {
                OnMissionComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MissionManager] OnMissionComplete handler threw: {ex}");
            }

            // MissionBlockSO advances the timeline when OnMissionComplete fires.


            return;
        }


        var task = _tasks[_currentIndex];

        taskUI.ResetTaskUI();
        taskUI?.SetTaskHeader(missionAsset.missionName,
                              task.title ?? string.Empty,
                              task.instruction ?? string.Empty,
                              _currentIndex + 1,
                              _tasks.Count
                              );

        if (_currentIndex + 1 < _tasks.Count)
        {
            var nextTask = _tasks[_currentIndex + 1];
            if (nextTask.taskType == MissionTaskType.GPS)
            {
                var db = missionAsset?.waypointDatabase;
                var nextWp = db?.GetById(nextTask.waypointId);
                if (nextWp != null)
                {
                    gpsUI.SetNextWaypointInfo(nextWp.latitude, nextWp.longitude, nextTask.title);
                }
            }
        }

        switch (task.taskType)
        {
            case MissionTaskType.GPS:
                StartGpsTask(task);
                break;
            case MissionTaskType.POI:
                StartPoiTask(task);
                break;
            case MissionTaskType.Question:
                StartQuestionTask(task);
                break;
            case MissionTaskType.TextInput:
                StartTextInputTask(task);
                break;
            case MissionTaskType.Custom:
                StartCustomTask(task);
                break;
        }

        Debug.Log($"[MissionManager] Starting task #{_currentIndex}: type={task.taskType}");
        OnTaskChanged?.Invoke(task, _currentIndex, _tasks.Count);

        if (_taskCompleted)
        {
            Debug.LogWarning("[MissionManager] ⚠️ Task was somehow already marked completed before starting? This should never happen.");
        }

    }

    public void CompleteCurrentTask()
    {
        Debug.Log($"[MissionManager] >>> Attempting CompleteCurrentTask() — _taskCompleted={_taskCompleted}");

        if (_taskCompleted)
        {
            Debug.LogWarning("[MissionManager] ⚠️ Task already marked as completed — skipping");
            return;
        }

        _taskCompleted = true;
        Debug.Log("[MissionManager] ✅ Marked _taskCompleted = true and beginning completion flow");

        if (_currentIndex >= 0 && _currentIndex < _tasks.Count)
        {
            _tasks[_currentIndex].isCompleted = true;

            var task = _tasks[_currentIndex];

            taskUI.ShowHeader(false);
            taskUI.ShowQuestionSection(false);
            taskUI.ShowTextInputSection(false);
            // taskUI.ShowWrapperContainerOnly(true);
            taskUI.SetTaskCompletionContainer(task.completionTitle, task.completionQuote);
            taskUI.ShowCompletionSection(true);

            var postCompletionDelay = _tasks[_currentIndex].taskCompletionDelay;
            if (postCompletionDelay <= 0f)
            {
                Debug.LogWarning($"[MissionManager] ⚠️ Task {task.title} has no post-completion delay set — using fallback 2.5s");
                postCompletionDelay = 2.5f;
            }
            StartCoroutine(DelayedAdvanceToNextTask(postCompletionDelay));

        }
    }

    private IEnumerator DelayedAdvanceToNextTask(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        taskUI.ShowCompletionSection(false);

        if (enableScoringInThisMission)
        {
            ScoreManager.Instance.AddTaskPoints();
            Debug.Log($"[MissionManager] +{pts} poena za završetak zadatka");

            if (scoreUI != null)
            {
                Debug.Log("[MissionManager] Waiting for ScoreUI toast to complete...");
                yield return scoreUI.WaitForToastToComplete();
            }
        }

        AdvanceToNextTask();
    }


    #region Task Starters

    private void StartGpsTask(MissionTask task)
    {
        Debug.Log("[MissionManager] Starting GPS Task");

        if (waypointManager == null)
        {
            Debug.LogError("[MissionManager] ❌ Cannot start GPS task, WaypointManager is null");
            return;
        }

        if (string.IsNullOrEmpty(task.waypointId))
        {
            Debug.LogError("[MissionManager] ❌ gpsWaypointId is null or empty. Task setup is broken.");
            return;
        }

        Debug.Log($"[MissionManager] GPS handler expecting → id: {task.waypointId}, zone: {task.confirmationZone}");

        // Also guard against invalid enum
        if (!Enum.IsDefined(typeof(WaypointZoneLevel), task.confirmationZone))
        {
            Debug.LogError("[MissionManager] ❌ Invalid confirmationZone enum value: " + task.confirmationZone);
            return;
        }

        taskUI.ShowHeader(true);
        taskUI.ShowQuestionSection(false);
        taskUI.ShowTextInputSection(false);


        // Waypoint-driven system — add only runtime marker if we need a visual cue (optional)
        var db = missionAsset?.waypointDatabase;
        var targetWaypoint = db?.GetById(task.waypointId);
        if (targetWaypoint == null)
        {
            Debug.LogError($"[MissionManager] Waypoint ID not found in database: {task.waypointId}");
            return;
        }

        gpsUI.Show();
        // gpsUI.SetMissionTarget(targetWaypoint.latitude, targetWaypoint.longitude, task.title);
        // gpsUI.SetActiveTarget(targetWaypoint.latitude, targetWaypoint.longitude, task.title, WaypointTargetSource.Mission);

        gpsUI?.SetActiveTarget(targetWaypoint, WaypointTargetSource.Mission);


        if (!waypointManager.ContainsWaypoint(task.waypointId))
        {
            var runtimeWp = new WaypointRuntime(targetWaypoint);
            runtimeWp.zone3ConfirmDelay = 1f;
            waypointManager.AddWaypoint(runtimeWp);
        }


        // Register GPS logic using zone trigger
        _activeWaypointId = task.waypointId;
        gpsWaypointHandler = (id, zone) =>
        {
            Debug.Log($"[MissionManager] GPS handler fired for id={id}, zone={zone} (expecting id={_activeWaypointId}, zone={task.confirmationZone})");
            if (id == _activeWaypointId && zone == task.confirmationZone)
            {
                Debug.Log($"✅ Reached required zone {zone} of waypoint {id} — completing task");
                waypointManager.OnWaypointZoneEntered -= gpsWaypointHandler;
                _completedWaypointTarget = targetWaypoint;
                CompleteCurrentTask();
            }
        };


        waypointManager.OnWaypointZoneEntered += gpsWaypointHandler;
        var currentLevel = waypointManager.GetCurrentZoneLevelFor(task.waypointId);
        Debug.Log($"[MissionManager] 🔎 Immediate zone check → {currentLevel}");

        if (currentLevel == task.confirmationZone)
        {
            Debug.Log($"[MissionManager] ✅ Already in required zone {currentLevel} — completing task immediately");
            waypointManager.OnWaypointZoneEntered -= gpsWaypointHandler;
            _completedWaypointTarget = targetWaypoint;
            CompleteCurrentTask();

        }

    public bool IsWaypointLockedByCurrentMission(string waypointId)
    {
        if (MissionAsset == null || MissionAsset.tasks == null)
            return false;

        foreach (var task in MissionAsset.tasks)
        {
            if (task.taskType == MissionTask.MissionTaskType.GPS &&
                !task.isCompleted &&
                task.waypointId == waypointId)
            {
                return true;
            }
        }

        return false;

    }

    private IEnumerator CheckProximity(MissionTask task)
    {
        // Optional fallback: still show directional arrow if needed
        var db = missionAsset?.waypointDatabase;
        var targetWaypoint = db?.GetById(task.waypointId);
        if (targetWaypoint == null)
            yield break;

        double targetLat = targetWaypoint.latitude;
        double targetLon = targetWaypoint.longitude;

        bool shownUI = false, shownHint = false;
        yield return new WaitUntil(() => GpsProvider.IsGpsAvailable);

        while (true)
        {
            double lat = GpsProvider.Latitude;
            double lon = GpsProvider.Longitude;
            float dist = ComputeHaversineDistance(lat, lon, targetLat, targetLon);
            gpsUI.UpdateDistance(dist);

            if (!shownUI && dist < 300f)
            {
                Debug.Log("[MissionManager] Within 300m — showing task UI");
                shownUI = true;
            }
            if (!shownHint && dist < 100f)
            {
                Debug.Log("[MissionManager] Hint: Close to target!");
                shownHint = true;
            }

            // We no longer auto-complete based on distance
            // Only zone detection will trigger completion

            yield return new WaitForSeconds(1.5f);
        }
    }


    private void StartPoiTask(MissionTask task)
    {
        Debug.Log("[MissionManager] Starting POI Task");
        taskUI.ShowHeader(true);
        taskUI.ShowQuestionSection(false);
        taskUI.ShowTextInputSection(false);

        poiDetector.enabled = true;
        poiDetector.interactionDistance = poiInteractionDistance;

        Action<POIExtendedController> onActivation = null;
        onActivation = ext =>
        {
            POIFleetManager.Instance.OnPOIActivationStarted -= onActivation;
            poiConfirmedHandler = confirmedExt => HandlePoiInteraction(confirmedExt);
            poiDetector.OnPOIConfirmed += poiConfirmedHandler;
        };
        POIFleetManager.Instance.OnPOIActivationStarted += onActivation;
        Debug.Log($"[MissionManager] Requesting activation of POI '{task.poiIdentifier}'");
        POIFleetManager.Instance.ActivatePOIById(task.poiIdentifier);
    }

    private void HandlePoiInteraction(POIExtendedController poiExt)
    {
        poiDetector.OnPOIConfirmed -= poiConfirmedHandler;
        poiDetector.enabled = false;
        Debug.Log($"[MissionManager] HandlePoiInteraction for POI '{poiExt.definition.id}'");

        var dialogAsset = poiExt.definition.dialogAsset;
        if (dialogAsset != null)
        {
            Debug.Log($"[MissionManager] Injecting dialogue '{dialogAsset.name}'");
            DialogueManager.Instance.StartInjectedDialogue(
                dialogAsset,
                () => OnInjectedDialogueComplete(poiExt.definition.id)
            );
        }
        else
        {
            Debug.Log($"[MissionManager] No DialogAsset on POI '{poiExt.definition.id}' — skipping dialogue");
            CompleteCurrentTask();
        }
    }

    private void OnInjectedDialogueComplete(string poiId)
    {
        Debug.Log($"[MissionManager] Injected dialogue complete for POI '{poiId}'");
        CompleteCurrentTask();
    }

    private void StartQuestionTask(MissionTask task)
    {
        Debug.Log("[MissionManager] Starting Question Task");
        taskUI.ShowHeader(true);
        taskUI.ShowTextInputSection(false);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(DelayedQuestionSetup(task));
        taskUI.ShowQuestionSection(true);
    }

    private IEnumerator DelayedQuestionSetup(MissionTask task)
    {
        yield return null;

        var choices = task.Choices ?? Array.Empty<string>();
        int n = choices.Length;

        // Flexible path when not exactly 2
        if (n != 2)
        {
            taskUI.ConfigureQuestionUIFlexible(
                task.questionText,
                choices,
                task.CorrectChoiceIndex,
                isCorrect =>
                {
                    if (isCorrect)
                    {
                        Debug.Log("[MissionManager] Correct answer (flex), completing task");
                        CompleteCurrentTask();
                    }
                    else
                    {
                        Debug.Log("[MissionManager] Incorrect answer (flex), prompting retry");
                        taskUI.ShowChoiceError("Pogrešan odgovor - pokušaj ponovo!");
                        if (enableScoringInThisMission)
                        {
                            ScoreManager.Instance.DeductWrongAnswer();
                            Debug.Log($"[MissionManager] -{pen} poena za netačan odgovor");
                        }
                    }
                }
            );
            yield break;
        }

        // Legacy A/B path (unchanged)
        taskUI.ConfigureQuestionUI(
            task.questionText,
            choices.Length > 0 ? choices[0] : "A",
            choices.Length > 1 ? choices[1] : "B",
            task.CorrectChoiceIndex,
            isCorrect =>
            {
                if (isCorrect)
                {
                    Debug.Log("[MissionManager] Correct answer, completing task");
                    CompleteCurrentTask();
                }
                else
                {
                    Debug.Log("[MissionManager] Incorrect answer, prompting retry");
                    taskUI.ShowChoiceError("Pogrešan odgovor - pokušaj ponovo!");

                    if (enableScoringInThisMission)
                    {
                        ScoreManager.Instance.DeductWrongAnswer();
                        Debug.Log($"[MissionManager] -{pen} poena za netačan odgovor");
                    }
                }
            }
        );
    }


    private void StartTextInputTask(MissionTask task)
    {
        Debug.Log("[MissionManager] Starting Text Input Task");
        taskUI.ShowHeader(true);
        taskUI.ShowQuestionSection(false);
        taskUI.ShowTextInputSection(true);
        taskUI.ConfigureTextInputUI(
            task.inputPlaceholder,
            task.answerPlaceholder,
            answerObj =>
            {
                string ans = answerObj as string ?? string.Empty;
                if (string.Equals(ans, task.expectedAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("[MissionManager] Correct input, completing task");
                    CompleteCurrentTask();
                }
                else
                {
                    Debug.Log("[MissionManager] Incorrect input, retrying");
                    taskUI.ShowInputError("Try again!");
                    if (enableScoringInThisMission)
                    {
                        ScoreManager.Instance.DeductWrongAnswer();
                        Debug.Log($"[MissionManager] -{pen} poena za netačan odgovor");
                    }
                }
            }
        );
    }

    private void StartCustomTask(MissionTask task)
    {
        Debug.Log($"[MissionManager] Starting custom task: {task.customData}");
    }

    #endregion

    private void CleanupPreviousTask()
    {
        if (gpsWaypointHandler != null)
        {
            waypointManager.OnWaypointZoneEntered -= gpsWaypointHandler;
            gpsWaypointHandler = null;
            Debug.Log("[MissionManager] Cleaned up GPS handler");
        }
        if (poiConfirmedHandler != null)
        {
            poiDetector.OnPOIConfirmed -= poiConfirmedHandler;
            poiConfirmedHandler = null;
            Debug.Log("[MissionManager] Cleaned up POI handler");
        }
    }

    private void OnWaypointReached(string waypointId)
    {
        Debug.Log($"[MissionManager] OnWaypointReached: {waypointId}");
        if (waypointId == _activeWaypointId)
        {
            Debug.Log("[MissionManager] Matching waypoint reached — completing task");
            CompleteCurrentTask();
        }
    }

    public void MarkWaypointAsCompleted(string waypointId)
    {
        if (!completedWaypointIds.Contains(waypointId))
        {
            completedWaypointIds.Add(waypointId);
            Debug.Log($"[MissionManager] Waypoint {waypointId} marked as completed.");
        }
    }

    public bool HasActiveMission => missionAsset != null && _tasks != null && _tasks.Count > 0 && _currentIndex >= 0;

    public (MissionTask task, int index, int total) GetCurrentTaskSnapshot()
    {
        if (!HasActiveMission) return (null, -1, 0);
        return (_tasks[_currentIndex], _currentIndex + 1, _tasks.Count);
    }

    public bool IsWaypointCompleted(string waypointId)
    {
        return completedWaypointIds.Contains(waypointId);
    }

    /// <summary>
    /// Distance between two lat/lon points using the Haversine formula.
    /// </summary>
    private float ComputeHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000.0;
        double dLat = Mathf.Deg2Rad * (lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Mathf.Deg2Rad * lat1) * Math.Cos(Mathf.Deg2Rad * lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (float)(R * c);
    }
}
