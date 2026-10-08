using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using Binding = WaypointTriggerBinding.Binding;
using MyGame.Timeline;
<<<<<<< Updated upstream
using System.Collections;
=======
>>>>>>> Stashed changes

[RequireComponent(typeof(MasterTimelineManager))]
public class TriggerEvaluator : MonoBehaviour
{
    // Managers & controllers
    // AR Image support
    ARTrackedImageManager _arImageManager;

    // Core managers
<<<<<<< Updated upstream
    [Header("Timeline Reference")]
    [Tooltip("ScriptableObject holding the timeline database of blocks.")]
    [SerializeField] private TimelineSettings timelineSettings;
    [SerializeField] private MasterTimelineManager timelineReference;
    [SerializeField] private MissionManager missionReference;
    MissionManager _missionMgr;
    MasterTimelineManager _timeline;

    public static event Action<HintBlockSO> OnHintTriggered;


=======
    MissionManager _missionMgr;
    MasterTimelineManager _timeline;

>>>>>>> Stashed changes
    [SerializeField]
    private WaypointTriggerBinding _triggerBindingAsset;
    public WaypointTriggerBinding Binding => _triggerBindingAsset;

    // Flattened list of every condition (was missing)
    private List<TimelineTriggerCondition> _conditions;

    // cache of each binding → its generated group key
    private Dictionary<Binding, string> _groupKeyCache;

    // GPS HUD for time-based conditions
    private MissionGPSHUDUIController _hudController;

    // What’s already fired/running
    private readonly HashSet<string> _triggeredGroups = new();
    private readonly Dictionary<Binding, HashSet<WaypointZoneLevel>> _reachedZones = new();
    private readonly HashSet<string> _firedEvents = new();
<<<<<<< Updated upstream
    // private readonly HashSet<string> _completedMissionNames = new();
    // private readonly HashSet<string> _completedBlocks = new();

    public bool HasFired(string key) => _triggeredGroups.Contains(key);

   /* private void Start()
    {
        if (_timeline == null && MasterTimelineManager.Instance != null)
        {
            _timeline = MasterTimelineManager.Instance;
            _timeline.OnBlockStarted += HandleBlockStarted;
            Debug.Log("[TriggerEvaluator] 🔁 Late-linked timeline reference via Start()");
        }
    }
   */
=======

    public bool HasFired(string key) => _triggeredGroups.Contains(key);

>>>>>>> Stashed changes
    // ——————— Lifecycle ————————

    private void OnEnable()
    {
        // 1) Build flat list of all conditions
        _conditions = _triggerBindingAsset
            .waypointTriggers
            .SelectMany(b => b.triggers)
            .ToList();

        // 2) Cache group‐keys so you never regenerate them
        _groupKeyCache = new Dictionary<Binding, string>(_triggerBindingAsset.waypointTriggers.Count);
        foreach (var b in _triggerBindingAsset.waypointTriggers)
            _groupKeyCache[b] = GenerateGroupKeyFromBundle(b);

        // 3) WaypointManager hookup
        var wpManager = FindFirstObjectByType<WaypointManager>();
        if (wpManager != null)
        {
            wpManager.OnWaypointZoneEntered += HandleWaypointZoneEntered;
            Debug.Log("[TriggerEvaluator] ✅ Bound to WaypointManager");
        }

        // 4) GPS HUD for time tasks
        _hudController = FindFirstObjectByType<MissionGPSHUDUIController>();
        if (_hudController == null)
            Debug.LogError("[TriggerEvaluator] ❌ Could not find MissionGPSHUDUIController");

        // 5) Mission events
<<<<<<< Updated upstream
        _missionMgr = missionReference != null ? missionReference : MissionManager.Instance;

        if (_missionMgr != null)
        {
            Debug.Log($"[TriggerEvaluator] ✅ Subscribed to MissionManager: {_missionMgr.name}");
            _missionMgr.OnMissionStarted += HandleMissionStarted;
            _missionMgr.OnMissionComplete += HandleMissionComplete;
        }
        else
        {
            Debug.LogError("[TriggerEvaluator] ❌ No MissionManager assigned or found!");
        }

        // 6) Timeline‐block events
        _timeline = timelineReference != null ? timelineReference : MasterTimelineManager.Instance;

        if (_timeline != null)
        {
            Debug.Log($"[TriggerEvaluator] ✅ Timeline hook registered: {_timeline.name}");
            _timeline.OnBlockStarted += HandleBlockStarted;
            _timeline.OnBlockEnded += HandleBlockEnded;
        }
        else
        {
            Debug.LogError("[TriggerEvaluator] ❌ Timeline reference is still null after OnEnable! Triggers may fail.");
        }

        if (timelineSettings == null)
            Debug.LogError("[TriggerEvaluator] TimelineSettings asset is not assigned in inspector.", this);
=======
        _missionMgr = MissionManager.Instance;
        if (_missionMgr != null)
        {
            _missionMgr.OnMissionStarted += HandleMissionStarted;
            _missionMgr.OnMissionComplete += HandleMissionComplete;
        }

        // 6) Timeline‐block events
        _timeline = MasterTimelineManager.Instance;
        if (_timeline != null)
        {
            _timeline.OnBlockStarted += HandleBlockStarted;
            _timeline.OnBlockEnded += HandleBlockEnded;
        }
>>>>>>> Stashed changes

        // 7) AR Image tracking (Foundation 6+)
        _arImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        if (_arImageManager != null)
            _arImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        // Unhook everything in reverse order

        var wpManager = FindFirstObjectByType<WaypointManager>();
        if (wpManager != null)
            wpManager.OnWaypointZoneEntered -= HandleWaypointZoneEntered;

        if (_missionMgr != null)
        {
            _missionMgr.OnMissionStarted -= HandleMissionStarted;
            _missionMgr.OnMissionComplete -= HandleMissionComplete;
        }

        if (_timeline != null)
        {
            _timeline.OnBlockStarted -= HandleBlockStarted;
            _timeline.OnBlockEnded -= HandleBlockEnded;
        }

        if (_arImageManager != null)
            _arImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);

        // Clear all caches and state
        _groupKeyCache.Clear();
        _conditions.Clear();
        _triggeredGroups.Clear();
        _firedEvents.Clear();
        _reachedZones.Clear();
<<<<<<< Updated upstream
        // _completedMissionNames.Clear();
        // _completedBlocks.Clear();
=======
>>>>>>> Stashed changes
    }

    private void HandleWaypointZoneEntered(string waypointId, WaypointZoneLevel zone)
    {
        Debug.Log($"[TriggerEvaluator] 📍 Waypoint entered: {waypointId} in zone {zone}");

        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
<<<<<<< Updated upstream
            if (binding == null)
            {
                Debug.LogWarning("[TriggerEvaluator] ⚠️ Skipping null binding in waypointTriggers.");
                continue;
            }

=======
>>>>>>> Stashed changes
            if (string.IsNullOrEmpty(binding.waypointId))
                continue;

            if (binding.waypointId != waypointId)
                continue;

            // 1) Track that we’ve now hit this zone for that binding:
            if (!_reachedZones.TryGetValue(binding, out var zones))
            {
                zones = new HashSet<WaypointZoneLevel>();
                _reachedZones[binding] = zones;
            }
            zones.Add(zone);

            // 2) Skip if this group already fired once and is one-shot:
            string groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey))
            {
                Debug.Log($"[TriggerEvaluator] 🔁 Skipping already triggered: {groupKey}");
                continue;
            }

            // 3) (Optionally) log your “activation match” at the first configured zone:
            if (zone == binding.activationZone)
                Debug.Log($"[TriggerEvaluator] 🔓 Activation match for group targeting block {binding.targetBlockIndex} (groupKey={groupKey})");

            // 4) Evaluate *all* conditions against the full history of zones:
            bool allConditionsMet = EvaluateConditionGroup(binding, zones);
            if (allConditionsMet)
            {
                Debug.Log($"[TriggerEvaluator] 🎯 All conditions met! Firing block {binding.targetBlockIndex} for groupKey={groupKey}");
                _triggeredGroups.Add(groupKey);
<<<<<<< Updated upstream

                if (binding.targetBlockIndex < 0)
                {
                    Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                    return;
                }
                // Učitaj metapodatke o bloku iz TimelineSettings
                int idx = binding.targetBlockIndex;

                // Validate index against the database
                var db = timelineSettings.timelineDatabase;
                if (db == null || idx < 0 || idx >= db.blocks.Count)
                {
                    Debug.LogError($"[TriggerEvaluator] Invalid block index {idx} for hint evaluation.");
                    continue;
                }

                // Retrieve the block SO directly from the list
                var blockSO = db.blocks[idx];

                // If it's a hint block, emit hint event; otherwise request timeline execution
                if (blockSO.blockType == TimelineBlockSO.BlockType.Hint && blockSO is HintBlockSO hint)
                {
                    OnHintTriggered?.Invoke(hint);
                }
                else
                {
                    _timelineSafety.RequestBlockExecution(idx);
                }

=======
                _timeline.RequestBlockExecution(binding.targetBlockIndex);
>>>>>>> Stashed changes
            }
            else
            {
                // Log which ones are still missing, if you want:
                Debug.Log($"[TriggerEvaluator] ⛔ Conditions not yet met for groupKey={groupKey}");
            }
<<<<<<< Updated upstream

            EvaluateAllConditionGroups();
=======
>>>>>>> Stashed changes
        }
    }


    //–– Handlers that forward into your trigger system ––
    private void HandleMissionStarted()
    => TriggerByMissionEvent(_missionMgr.MissionAsset.missionName,
                             TriggerSubEventType.OnMissionStarted);

    private void HandleMissionComplete()
        => TriggerByMissionEvent(_missionMgr.MissionAsset.missionName,
                                 TriggerSubEventType.OnMissionComplete);

<<<<<<< Updated upstream
    public void TriggerByMissionEvent(string missionId, TriggerSubEventType subType)
    {
        string key = $"MISSION_{subType}_{missionId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate Mission event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 Mission event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
    }

    private void HandleBlockStarted(ITimelineBlock block)
    {
        if (block is TimelineBlockSO blockSO)
            TriggerByGameBlockEvent(blockSO.blockId, TriggerSubEventType.OnBlockStarted);
=======
    private void HandleBlockStarted(ITimelineBlock block)
    {
        var id = (block as TimelineBlockSO)?.blockId;
        TriggerByGameBlockEvent(id, TriggerSubEventType.OnBlockStarted);
>>>>>>> Stashed changes
    }

    private void HandleBlockEnded(ITimelineBlock block)
    {
<<<<<<< Updated upstream
        if (block is TimelineBlockSO blockSO)
            TriggerByGameBlockEvent(blockSO.blockId, TriggerSubEventType.OnBlockEnded);
    }

    public void TriggerByGameBlockEvent(string blockId, TriggerSubEventType subType)
    {
        string key = $"BLOCK_{subType}_{blockId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate GameBlock event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 GameBlock event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
    }

    public void TriggerByDialogEvent(string dialogId, TriggerSubEventType subType)
    {
        string key = $"DIALOG_{subType}_{dialogId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate Dialog event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 Dialog event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
    }

    public void TriggerByPOIEvent(string poiId, TriggerSubEventType subType)
    {
        string key = $"POI_{subType}_{poiId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate POI event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 POI event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
    }

    public void TriggerByUIEvent(string uiEventId, TriggerSubEventType subType)
    {
        string key = $"UI_{subType}_{uiEventId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate UI event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 UI event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
=======
        var id = (block as TimelineBlockSO)?.blockId;
        TriggerByGameBlockEvent(id, TriggerSubEventType.OnBlockEnded);
>>>>>>> Stashed changes
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var img in args.added)
<<<<<<< Updated upstream
            TriggerByTrackedImageEvent(img.referenceImage.name,
=======
            TriggerByImageEvent(img.referenceImage.name,
>>>>>>> Stashed changes
                                TriggerSubEventType.OnImageTracked);

        foreach (var pair in args.removed)
        {
            var img = pair.Value;  // <-- unwrap the KeyValuePair
<<<<<<< Updated upstream
            TriggerByTrackedImageEvent(img.referenceImage.name,
=======
            TriggerByImageEvent(img.referenceImage.name,
>>>>>>> Stashed changes
                                TriggerSubEventType.OnImageLost);
        }
    }

<<<<<<< Updated upstream
    public void TriggerByTrackedImageEvent(string imageId, TriggerSubEventType subType)
    {
        string key = $"IMAGE_{(int)subType}_{imageId}";
        if (_firedEvents.Contains(key))
        {
            Debug.Log($"[TriggerEvaluator] 🔁 Duplicate TrackedImage event ignored: {key}");
            return;
        }

        Debug.Log($"[TriggerEvaluator] 🧠 TrackedImage event fired: {key}");
        _firedEvents.Add(key);
        EvaluateAllConditionGroups();
    }

    [ContextMenu("Print Fired Events")]
    private void PrintFiredEvents()
    {
        Debug.Log($"[TriggerEvaluator] 🔍 Fired events so far:\n  - {string.Join("\n  - ", _firedEvents)}");
    }
=======
>>>>>>> Stashed changes


    private bool EvaluateConditionGroup(
     WaypointTriggerBinding.Binding binding,
     HashSet<WaypointZoneLevel> reachedZones)
    {
        // which waypoint we’re on
        string enteredWaypoint = binding.waypointId;

        // grab your time values once
        int _lastGlobalSecondsEvaluator = _hudController != null
            ? _hudController.GlobalSeconds
            : -1;
        int _lastBlockSecondsEvaluator = _hudController != null
            ? _hudController.LastBlockSeconds
            : -1;

        foreach (var condition in binding.triggers)
        {
            // same label logic
            string label = !string.IsNullOrEmpty(condition.conditionId)
                   ? condition.conditionId
                   : (string.IsNullOrEmpty(condition.label)
                       ? $"Auto_{condition.triggerType}"
                       : condition.label);

            string conWaypointId = string.IsNullOrEmpty(condition.conWaypointId)
                ? binding.waypointId
                : condition.conWaypointId;

            Debug.Log(
                $"[TriggerEvaluator] 🧠 Checking condition {{GetLogPrefix(binding)}}: " +
                $"type={condition.triggerType}, " +
                $"conWaypointId={conWaypointId}, " +
                $"requiredZone={condition.requiredZone}"
            );

            switch (condition.triggerType)
            {
                case TriggerType.Waypoint:
                    // still require a valid waypoint reference
                    if (string.IsNullOrEmpty(conWaypointId))
                    {
                        Debug.LogWarning(
                            $"[TriggerEvaluator] ⚠️ Condition '{GetLogPrefix(binding)}' " +
                            "missing conWaypointId, skipping."
                        );
                        return false;
                    }

                    // must be for the same waypoint
                    if (conWaypointId != enteredWaypoint)
                    {
                        Debug.Log(
                            $"[TriggerEvaluator] ⛔ Condition '{GetLogPrefix(binding)}' " +
                            $"waypoint mismatch (expected {enteredWaypoint})."
                        );
                        return false;
                    }

                    // and must have ever entered that zone
                    if (!reachedZones.Contains(condition.requiredZone))
                    {
                        Debug.Log(
                            $"[TriggerEvaluator] ⛔ Condition '{GetLogPrefix(binding)}' " +
                            $"zone {condition.requiredZone} not yet reached."
                        );
                        return false;
                    }

                    Debug.Log(
<<<<<<< Updated upstream
                        $"[TriggerEvaluator] ✅ Waypoint {enteredWaypoint} {condition.requiredZone} Condition '{GetLogPrefix(binding)}' met"
=======
                        $"[TriggerEvaluator] ✅ Condition '{GetLogPrefix(binding)}' met"
>>>>>>> Stashed changes
                    );
                    break;

                case TriggerType.Time:
                    if (condition.subEventType == TriggerSubEventType.GlobalTime)
                    {
                        if (_lastGlobalSecondsEvaluator < condition.requiredSeconds)
                        {
                            Debug.Log(
                                $"[TriggerEvaluator] ⏱️ Global time not yet passed " +
                                $"for condition '{GetLogPrefix(binding)}'"
                            );
                            return false;
                        }
                        Debug.Log(
                            $"[TriggerEvaluator] ✅ GlobalTime condition '{GetLogPrefix(binding)}' met"
                        );
                    }
                    else if (condition.subEventType == TriggerSubEventType.BlockTime)
                    {
                        if (_lastBlockSecondsEvaluator < condition.requiredSeconds)
                        {
                            Debug.Log(
                                $"[TriggerEvaluator] ⏱️ Block time not yet passed " +
                                $"for condition '{GetLogPrefix(binding)}'"
                            );
                            return false;
                        }
                        Debug.Log(
                            $"[TriggerEvaluator] ✅ BlockTime condition '{GetLogPrefix(binding)}' met"
                        );
                    }
                    break;

                case TriggerType.POI:
<<<<<<< Updated upstream
                    {
                        if (!_firedEvents.Contains(condition.conditionId))
                        {
                            Debug.LogWarning($"[TriggerEvaluator] ⛔ POI '{condition.poiId}' not marked as fired under conditionId='{condition.conditionId}'");
                            return false;
                        }
                        Debug.Log($"[TriggerEvaluator] ✅ POI '{condition.poiId}' condition met");
                        break;
                    }

                case TriggerType.Dialog:
                    {
                        if (!_firedEvents.Contains(condition.conditionId))
                        {
                            Debug.LogWarning($"[TriggerEvaluator] ⛔ Dialog '{condition.dialogId}' not marked as fired under conditionId='{condition.conditionId}'");
                            return false;
                        }
                        Debug.Log($"[TriggerEvaluator] ✅ Dialog '{condition.dialogId}' condition met");
                        break;
                    }

=======
                case TriggerType.Dialog:
>>>>>>> Stashed changes
                case TriggerType.UI:
                    if (string.IsNullOrEmpty(condition.conditionId))
                    {
                        Debug.LogWarning($"[TriggerEvaluator] ⚠️ Condition at index … has no conditionId");
                        return false;
                    }
                    if (!_firedEvents.Contains(condition.conditionId))
                    {
                        Debug.Log($"[TriggerEvaluator] ⛔ Condition '{GetLogPrefix(binding)}' event not yet recorded");
                        return false;
                    }
                    Debug.Log($"[TriggerEvaluator] ✅ Event condition '{GetLogPrefix(binding)}' met");
                    break;

<<<<<<< Updated upstream
                case TriggerType.Mission:
                    if (!_firedEvents.Contains(condition.conditionId))
                    {
                        Debug.Log($"[TriggerEvaluator] ⛔ Mission condition '{GetLogPrefix(binding)}' not yet fired");
                        return false;
                    }
                    Debug.Log($"[TriggerEvaluator] ✅ Mission condition '{GetLogPrefix(binding)}' met");
                    break;

                case TriggerType.GameBlock:
                    if (!_firedEvents.Contains(condition.conditionId))
                    {
                        Debug.Log($"[TriggerEvaluator] ⛔ GameBlock condition '{GetLogPrefix(binding)}' not yet fired");
                        return false;
                    }
                    Debug.Log($"[TriggerEvaluator] ✅ GameBlock condition '{GetLogPrefix(binding)}' met");
                    break;

                case TriggerType.TrackedImage:
                    if (!_firedEvents.Contains(condition.conditionId))
                    {
                        Debug.Log($"[TriggerEvaluator] ⛔ TrackedImage condition '{GetLogPrefix(binding)}' not yet fired");
                        return false;
                    }
                    Debug.Log($"[TriggerEvaluator] ✅ TrackedImage condition '{GetLogPrefix(binding)}' met");
                    break;


=======
>>>>>>> Stashed changes
                default:
                    Debug.LogWarning(
                        $"[TriggerEvaluator] ⚠️ Unknown TriggerType " +
                        $"in condition '{GetLogPrefix(binding)}'"
                    );
                    return false;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
            }
        }

        // if we got here, every condition passed
        return true;
    }

<<<<<<< Updated upstream
    private void EvaluateAllConditionGroups()
    {
        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
            string groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey)) continue;

            if (!_reachedZones.TryGetValue(binding, out var zones))
            {
                zones = new HashSet<WaypointZoneLevel>();
                _reachedZones[binding] = zones;
            }

            if (EvaluateConditionGroup(binding, zones))
            {
                _triggeredGroups.Add(groupKey);
                if (binding.targetBlockIndex < 0)
                {
                    Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                    continue;
                }

                Debug.Log($"[TriggerEvaluator] ✅ All conditions met → Firing block {binding.targetBlockIndex}");

                // Učitaj metapodatke o bloku iz TimelineSettings
                int idx = binding.targetBlockIndex;

                // Validate index against the database
                var db = timelineSettings.timelineDatabase;
                if (db == null || idx < 0 || idx >= db.blocks.Count)
                {
                    Debug.LogError($"[TriggerEvaluator] Invalid block index {idx} for hint evaluation.");
                    continue;
                }

                // Retrieve the block SO directly from the list
                var blockSO = db.blocks[idx];

                // If it's a hint block, emit hint event; otherwise request timeline execution
                if (blockSO.blockType == TimelineBlockSO.BlockType.Hint && blockSO is HintBlockSO hint)
                {
                    OnHintTriggered?.Invoke(hint);
                }
                else
                {
                    _timelineSafety.RequestBlockExecution(idx);
                }

            }
        }
    }


=======
>>>>>>> Stashed changes

    private string GenerateGroupKeyFromBundle(WaypointTriggerBinding.Binding binding)
    {
        if (!string.IsNullOrEmpty(binding.groupId))
            return binding.groupId;

        // (Optional) fallback: recreate it here the same way
        var condIds = binding.triggers.Select(c => c.conditionId);
        return $"{binding.waypointId}_{binding.targetBlockIndex}_{string.Join("_", condIds)}";
    }

<<<<<<< Updated upstream
    /*
=======

>>>>>>> Stashed changes

    public void TriggerByIdAndSubEvent(string id, TriggerSubEventType subType)
    {
        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
<<<<<<< Updated upstream
            if (binding == null)
            {
                Debug.LogWarning("[TriggerEvaluator] ⚠️ Skipping null binding in TriggerByIdAndSubEvent.");
                continue;
            }
=======
>>>>>>> Stashed changes
            string groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey))
                continue;

            // 1) grab the zones we’ve recorded (or use an empty set if none)
            if (!_reachedZones.TryGetValue(binding, out var zones))
            {
                zones = new HashSet<WaypointZoneLevel>();
                // NOTE: we don't strictly need to add it back here
                // since waypoint conditions only care about what's been hit via HandleWaypointZoneEntered
            }

            // 2) see if this trigger event applies
            foreach (var condition in binding.triggers)
            {
                if (condition.subEventType == subType &&
                    (condition.poiId == id ||
                     condition.dialogId == id ||
                     condition.uiElementId == id))
                {
                    Debug.Log(
                        $"[TriggerEvaluator] 🚀 Trigger match found for " +
                        $"subType={subType}, id={condition.conditionId} in block {binding.targetBlockIndex}"
                    );

                    _firedEvents.Add(condition.conditionId);
                    // 3) pass our 'zones' set into EvaluateConditionGroup
                    if (EvaluateConditionGroup(binding, zones))
                    {
                        _triggeredGroups.Add(groupKey);
<<<<<<< Updated upstream
                        if (binding.targetBlockIndex < 0)
                        {
                            Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                            return;
                        }
                        _timelineSafety?.RequestBlockExecution(binding.targetBlockIndex);
=======
                        _timeline.RequestBlockExecution(binding.targetBlockIndex);
>>>>>>> Stashed changes
                    }
                    break;
                }
            }

        }
    }

    public void TriggerByMissionEvent(string missionName, TriggerSubEventType subType)
    {
        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
            var groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey)) continue;

            if (!_reachedZones.TryGetValue(binding, out var zones))
                zones = new HashSet<WaypointZoneLevel>();

            foreach (var condition in binding.triggers)
            {
                if (condition.triggerType == TriggerType.Mission
                 && condition.subEventType == subType
                 && condition.missionName == missionName)
                {
                    Debug.Log($"[TriggerEvaluator] 🚀 Mission '{missionName}' / {subType} matched → block {binding.targetBlockIndex}");
                    _firedEvents.Add(condition.conditionId);
                    if (EvaluateConditionGroup(binding, zones))
                    {
                        _triggeredGroups.Add(groupKey);
<<<<<<< Updated upstream
                        if (binding.targetBlockIndex < 0)
                        {
                            Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                            return;
                        }
                        _timelineSafety?.RequestBlockExecution(binding.targetBlockIndex);
=======
                        _timeline.RequestBlockExecution(binding.targetBlockIndex);
>>>>>>> Stashed changes
                    }
                    break;
                }
            }
        }
    }

    public void TriggerByGameBlockEvent(string blockId, TriggerSubEventType subType)
    {
        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
            var groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey)) continue;

            if (!_reachedZones.TryGetValue(binding, out var zones))
                zones = new HashSet<WaypointZoneLevel>();

            foreach (var condition in binding.triggers)
            {
<<<<<<< Updated upstream
                Debug.Log($"[TE DEBUG] Looking for blockId={blockId}, conditionId={condition.conditionId}, subType={condition.subEventType}, expected={subType}");
=======
>>>>>>> Stashed changes
                if (condition.triggerType == TriggerType.GameBlock
                 && condition.subEventType == subType
                 && condition.blockId == blockId)
                {
                    Debug.Log($"[TriggerEvaluator] 🚀 GameBlock '{blockId}' / {subType} matched → block {binding.targetBlockIndex}");
                    _firedEvents.Add(condition.conditionId);
                    if (EvaluateConditionGroup(binding, zones))
                    {
                        _triggeredGroups.Add(groupKey);
<<<<<<< Updated upstream
                        if (binding.targetBlockIndex < 0)
                        {
                            Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                            return;
                        }
                        _timelineSafety?.RequestBlockExecution(binding.targetBlockIndex);
=======
                        _timeline.RequestBlockExecution(binding.targetBlockIndex);
>>>>>>> Stashed changes
                    }
                    break;
                }
            }
        }
    }
<<<<<<< Updated upstream
    
=======
>>>>>>> Stashed changes

    public void TriggerByImageEvent(string imageName, TriggerSubEventType subType)
    {
        foreach (var binding in _triggerBindingAsset.waypointTriggers)
        {
            var groupKey = GenerateGroupKeyFromBundle(binding);
            if (_triggeredGroups.Contains(groupKey)) continue;

            if (!_reachedZones.TryGetValue(binding, out var zones))
                zones = new HashSet<WaypointZoneLevel>();

            foreach (var condition in binding.triggers)
            {
                if (condition.triggerType == TriggerType.TrackedImage
                 && condition.subEventType == subType
                 && condition.imageName == imageName)
                {
                    Debug.Log($"[TriggerEvaluator] 🚀 Image '{imageName}' / {subType} matched → block {binding.targetBlockIndex}");
                    _firedEvents.Add(condition.conditionId);

                    if (EvaluateConditionGroup(binding, zones))
                    {
                        _triggeredGroups.Add(groupKey);
<<<<<<< Updated upstream

                        if (binding.targetBlockIndex < 0)
                        {
                            Debug.LogWarning($"[TriggerEvaluator] Skipping trigger: Invalid block index {binding.targetBlockIndex}");
                            return;
                        }
                        _timelineSafety?.RequestBlockExecution(binding.targetBlockIndex);
=======
                        _timeline.RequestBlockExecution(binding.targetBlockIndex);
>>>>>>> Stashed changes
                    }
                    break;
                }
            }
        }
    }
<<<<<<< Updated upstream
    */
    private MasterTimelineManager _timelineSafety
    {
        get
        {
            if (_timeline == null)
            {
                _timeline = MasterTimelineManager.Instance;
                if (_timeline == null)
                {
                    Debug.LogError("[TriggerEvaluator] ❌ Timeline manager is still null! Cannot execute timeline logic.");
                }
            }
            return _timeline;
        }
    }
   
=======

>>>>>>> Stashed changes

    public void ResetAllTriggers()
    {
        Debug.Log("[TriggerEvaluator] 🔁 Resetting all triggered groups");
        _triggeredGroups.Clear();
        _firedEvents.Clear();
        _reachedZones.Clear();
    }

    private string GetLogPrefix(Binding b)
    {
        // pull groupKey from cache once
        var groupKey = _groupKeyCache.TryGetValue(b, out var key)
            ? key
            : GenerateGroupKeyFromBundle(b);

        var name = string.IsNullOrEmpty(b.bindingLabel)
            ? $"Block#{b.targetBlockIndex}"
            : $"{b.bindingLabel}";

        return $"[TriggerEvaluator][{b.targetBlockIndex}:{name}][Group={groupKey}]";
    }
}
<<<<<<< Updated upstream
/* private IEnumerator WaitForTimelineLink()
    {
        while (MasterTimelineManager.Instance == null)
            yield return null;

        _timeline = MasterTimelineManager.Instance;
        _timeline.OnBlockStarted += HandleBlockStarted;
        _timeline.OnBlockEnded += HandleBlockEnded;
        Debug.Log("[TriggerEvaluator] 🔁 Timeline instance linked via coroutine.");
    }
   */

/* private void HandleBlockEnded(ITimelineBlock block)
 {
     Debug.Log($"[TriggerEvaluator] 🔔 Block ended: {(block as TimelineBlockSO)?.blockId}");
     var id = (block as TimelineBlockSO)?.blockId;
     TriggerByGameBlockEvent(id, TriggerSubEventType.OnBlockEnded);
 }
*/
=======
>>>>>>> Stashed changes
