using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGame.Waypoint;
using MyGame.Timeline;
using TimelineBlockDB = MyGame.Timeline.TimelineBlockDatabase;

/// <summary>
/// Manages a sequence of TimelineBlockSO entries, supporting:
///  • Entry delays between blocks
///  • Branching via an overrideNextIndex per block
///  • A “global” timer tracking total elapsed time since app start
///  • Pause/Resume behavior that halts both running blocks and entry‐delay timers
///  • Events for OnBlockStarted and OnBlockEnded (useful for BGM, UI, analytics, etc.)
/// 
/// To use:
///  1. Create a ScriptableObject of type TimelineBlockSO (or a subclass) for each block (Mission, Dialog, Hint, Exploration, etc.).
///  2. In the Inspector of MasterTimelineManager, populate the “blockEntries” list. For each entry:
///     • block: the SO instance
///     • entryDelay: how long (in seconds) to wait before starting this block
///     • overrideNextIndex: (optional) if ≥ 0, jump to that index instead of currentIndex + 1 when this block finishes
///  3. Call StartTimeline() to begin. The manager will fire OnBlockStarted/OnBlockEnded around each block’s StartBlock/CompleteBlock.
///  4. PauseTimeline()/ResumeTimeline() will pause either the currently running block or the entry‐delay coroutine.
///  5. JumpToBlock(i) immediately stops whatever is running or waiting and starts block i right away (no delay).
/// 
/// Each TimelineBlockSO must:
///  • Implement the ITimelineBlock interface (Initialize, StartBlock, PauseBlock, ResumeBlock, StopBlock)
///  • Call CompleteBlock() (inherited from TimelineBlockSO) when it is done.
/// 
/// The manager itself tracks “totalTime” in Update() using unscaledDeltaTime (so it keeps counting even if Time.timeScale == 0).
/// </summary>
/// 

public interface ITimelineBlock
{
    event Action<ITimelineBlock> OnBlockCompleted;
    void Initialize(MasterTimelineManager manager);
    void StartBlock();
    void PauseBlock();
    void ResumeBlock();
    void StopBlock();
    void ResetBlock(); // NEW: Reset logic for reuse or restart
}

public interface IAudioPlayableBlock
{
    AudioClip BgmClip { get; }
    AudioClip SfxClip { get; }
}

namespace MyGame.Timeline
{
    public abstract class TimelineBlockSO : ScriptableObject, ITimelineBlock, IAudioPlayableBlock
    {
        public enum BlockType
        {
            Mission,
            Exploration,
            DialogStandalone,
            DialogInjected,
            Hint,
            Delay // may be deprecated
        }

        [Header("Classification")]
        public BlockType blockType = BlockType.Mission;

        public event Action<ITimelineBlock> OnBlockCompleted;
        protected MasterTimelineManager manager;

        [Header("Audio Settings")]
        public AudioClip bgmClip;
        public AudioClip sfxClip;
        [Tooltip("Allow this block to override current BGM.")]
        public bool overrideBgm = false;
        [Tooltip("Unique ID for referencing this block externally.")]
        public string blockId = "Block ID";

        public virtual AudioClip BgmClip => bgmClip;
        public virtual AudioClip SfxClip => sfxClip;

        public virtual void Initialize(MasterTimelineManager manager)
        {
            this.manager = manager;
        }

        public abstract void StartBlock();
        public virtual void PauseBlock() { }
        public virtual void ResumeBlock() { }
        public virtual void StopBlock() { }
        public virtual void ResetBlock() { }

        protected void CompleteBlock()
        {
            Debug.Log($"[TimelineBlock] Marked complete: {name} (ID: {blockId})");
            OnBlockCompleted?.Invoke(this);
        }

        public virtual void Enter()
        {
            Debug.Log($"[TimelineBlock] Entering block: {name} (ID: {blockId})");
        }

        public virtual void Exit()
        {
            Debug.Log($"[TimelineBlock] Exiting block: {name} (ID: {blockId})");
        }
    }

}

<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
public class MasterTimelineManager : MonoBehaviour
{
    public static MasterTimelineManager Instance { get; private set; }
    [Serializable]
    public struct BlockEntry
    {
        [Tooltip("The dialog block (Standalone or Injected) to play here.")]
        public TimelineBlockSO block;
        [Tooltip("Seconds to wait before starting this block.")]
        public float entryDelay;
        [Tooltip("If ≥0, overrides which block index to run after this one.")]
        public int overrideNextIndex;
    }

    [Header("Timeline Settings")]
    [SerializeField] private TimelineSettings timelineSettings;

    public TimelineBlockDB ActiveDatabase => timelineSettings?.timelineDatabase;
    public TimelineSettings TimelineSettings => timelineSettings;


    [Header("Timeline Sequence")]
    [SerializeField]
    private List<BlockEntry> blockEntries = new List<BlockEntry>();

    [Header("Global Settings")]
    [Tooltip("If true, the manager will log basic timeline events to the console.")]
    public bool verboseLogging = false;

    // Events for hooking in BGM, UI, analytics, etc.
    public event Action<ITimelineBlock> OnBlockStarted;
    public event Action<int, ITimelineBlock> OnIndexChanged;
    public event Action<ITimelineBlock> OnBlockEnded;


    /// <summary>
    /// True while any block coroutine is playing.
    /// </summary>
    public bool IsRunningBlock { get; private set; }


    private int currentIndex = -1;
    private ITimelineBlock currentBlock = null;
    private Coroutine enteryDelayCoroutine = null;
    private bool isPaused = false; // Indicates if timeline is paused (affects both blocks and entryDelay)

    [Tooltip("Index of fallback exploration block.")]
    public int explorationFallbackIndex = -1;

    [Header("Global UI")]
    [Tooltip("GPS HUD (lat/lon/distance)")]
    [SerializeField] private MissionGPSHUDUIController gpsUI;
    [Tooltip("UI controller for task text & progress")]
    [SerializeField] private MissionTaskUIController taskUI;


    // Tracks total time (in seconds) since app start (unaffected by timeScale).
    public float totalTime { get; private set; } = 0f;
    // NEW: the global time (totalTime) when the current block was launched
    private float blockStartGlobalTime;
    /// <summary>
    /// How many seconds have passed since the current block began.
    /// </summary>
    public float BlockElapsedTime => currentBlock != null ? totalTime - blockStartGlobalTime : 0f;

    public IReadOnlyList<BlockEntry> BlockEntries => blockEntries;


    #region Unity Lifecycle

    private void Update()
    {
        // Increment the “global” timer regardless of pause state.
        totalTime += Time.unscaledDeltaTime;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Starts the timeline from the first entry (index 0), applying its entryDelay.
    /// </summary>
    /// 
    /*
#if UNITY_EDITOR
    [ContextMenu("Start Timeline")]
#endif
    */
    private bool hasStarted = false;
    public void StartTimeline()
    {
        if (hasStarted) return;
        hasStarted = true;
        gpsUI?.Show();

        if (blockEntries.Count == 0)
        {
            if (verboseLogging) Debug.LogWarning("[Timeline] No blocks to run.");
            return;
        }

        // Reset indices
        currentIndex = -1;
        CleanupCurrentAndDelay();
        ResetAllBlocks();

        if (verboseLogging) Debug.Log("[Timeline] Starting timeline from index 0");

        gpsUI?.Show();
        taskUI.ShowHeader(false);
        ScheduleBlock(0);
    }

    /// <summary>
    /// Pauses either the currently running block or, if waiting on entryDelay, pauses that timer.
    /// </summary>
    public void PauseTimeline()
    {
        if (isPaused) return;

        isPaused = true;
        if (currentBlock != null)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Pausing block idx={currentIndex}");
            currentBlock.PauseBlock();
        }
        else if (enteryDelayCoroutine != null)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Pausing entryDelay for next block");
            // The delay coroutine polls isPaused, so it will stall automatically.
        }
    }

    /// <summary>
    /// Resumes from pause: either continues the current block or the entryDelay coroutine.
    /// </summary>
    public void ResumeTimeline()
    {
        if (!isPaused) return;

        isPaused = false;
        if (currentBlock != null)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Resuming block idx={currentIndex}");
            currentBlock.ResumeBlock();
        }
        else if (enteryDelayCoroutine != null)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Resuming entryDelay for next block");
            // Delay coroutine sees isPaused = false and continues accumulating.
        }
    }

    /// <summary>
    /// Immediately jumps to block at the given index (0-based), skipping any entryDelay.
    /// </summary>
    public void JumpToBlock(int index)
    {
        if (index < 0 || index >= blockEntries.Count)
        {
            Debug.LogError($"[Timeline] JumpToBlock: index {index} is out of range.");
            return;
        }

        if (verboseLogging) Debug.Log($"[Timeline] Jumping immediately to block idx={index}");
        CleanupCurrentAndDelay();
        LaunchBlock(index);
    }

    #endregion

    #region Internal Scheduling & Callbacks

    /// <summary>
    /// Schedules the block at 'index' to start after its entryDelay.
    /// </summary>
    private void ScheduleBlock(int index)
    {
        if (index < 0 || index >= blockEntries.Count)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Index {index} is out of range. Timeline finished.");
            return;
        }

        CleanupCurrentBlock(); // Ensure any previous block is fully cleaned up

        float delay = blockEntries[index].entryDelay;
        if (delay <= 0f)
        {
            // No delay: launch immediately
            LaunchBlock(index);
        }
        else
        {
            // Start a coroutine that waits, checking isPaused each frame
            enteryDelayCoroutine = StartCoroutine(EntryDelayCoroutine(index, delay));
        }
    }

    /// <summary>
    /// Performs a Wait‐loop for 'delay' seconds (scaled time). If isPaused, it stalls.
    /// When complete, calls LaunchBlock(index).
    /// </summary>
    private IEnumerator EntryDelayCoroutine(int index, float delay)
    {
        float elapsed = 0f;
        if (verboseLogging) Debug.Log($"[Timeline] EntryDelay for block idx={index} → {delay} s");

        while (elapsed < delay)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        enteryDelayCoroutine = null;
        LaunchBlock(index);
    }

    /// <summary>
    /// Initializes, subscribes, and starts the block at 'index' immediately (no further delay).
    /// Fires OnBlockStarted before StartBlock().
    /// </summary>
    private void LaunchBlock(int index)
    {
        if (index < 0 || index >= blockEntries.Count) return;

        currentIndex = index;
        currentBlock = blockEntries[index].block;

        if (currentBlock == null)
        {
            Debug.LogError($"[Timeline] BlockEntry at index {index} has no assigned block SO.");
            return;
        }

        var blockName = (currentBlock as TimelineBlockSO)?.name ?? "Unnamed Block";
        Debug.Log($"[Timeline] Launching block idx={currentIndex} → {blockName}");

        if (currentBlock is IAudioPlayableBlock audioBlock)
        {
            if ((currentBlock as TimelineBlockSO)?.overrideBgm == true && audioBlock.BgmClip != null)
                if (BackgroundMusicManager.Instance.LastClipPlayed != audioBlock.BgmClip)
                {
                    BackgroundMusicManager.Instance.CrossfadeTo(audioBlock.BgmClip);
                }

            if (audioBlock.SfxClip != null)
                AudioSource.PlayClipAtPoint(audioBlock.SfxClip, Vector3.zero);
        }

        blockStartGlobalTime = totalTime;

        // Initialize and subscribe for completion
        IsRunningBlock = true;
        currentBlock.Initialize(this);
        currentBlock.OnBlockCompleted += HandleBlockCompleted;

        OnIndexChanged?.Invoke(currentIndex, currentBlock);
        // Fire the “started” event
        OnBlockStarted?.Invoke(currentBlock);

        // Start it
        currentBlock.StartBlock();
        if (currentBlock is MissionBlockSO)
        {
            UnifiedGpsProvider gps = FindFirstObjectByType<UnifiedGpsProvider>();
<<<<<<< Updated upstream
            if (gps != null) gps.emitInterval = 1f;
=======
            if (gps != null) gps.emitInterval = 10f;
>>>>>>> Stashed changes
        }
        else
        {
            UnifiedGpsProvider gps = FindFirstObjectByType<UnifiedGpsProvider>();
<<<<<<< Updated upstream
            if (gps != null) gps.emitInterval = 1f;
=======
            if (gps != null) gps.emitInterval = 1.5f;
>>>>>>> Stashed changes
        }
    }

    /// <summary>
    /// Called when a block calls CompleteBlock().
    /// Unsubscribes, fires OnBlockEnded, then decides next index (branching) and schedules it.
    /// </summary>
    /// 

    /// <summary>
    /// Expose your internal block-starting logic under a public name
    /// so other controllers can “request” a jump into a block.
    /// </summary>
    private readonly Queue<int> pendingRequests = new Queue<int>();
    public IReadOnlyList<TimelineBlockSO> ScheduledBlocks
        => pendingRequests.Select(i => blockEntries[i].block).ToList().AsReadOnly();

    public void ClearScheduledRequests() => pendingRequests.Clear();

    // …  

    public void RequestBlockExecution(int index)
    {
        // Sanity-check the index
        if (index < 0 || index >= blockEntries.Count)
        {
            Debug.LogError($"[Timeline] RequestBlockExecution: index {index} is out of range.");
            return;
        }

        // Pull the block reference out of the struct, then null-check it
        var block = blockEntries[index].block;
        if (block == null)
        {
            Debug.LogWarning($"[Timeline] RequestBlockExecution: no block assigned at index {index}.");
            return;
        }
        // If we can’t launch now, enqueue rather than drop
        if (IsRunningBlock && IsMutuallyExclusive(block.blockType))
        {
            Debug.Log($"[Timeline] Queuing block idx={index} → {block.name}");
            pendingRequests.Enqueue(index);
            return;
        }

        ScheduleBlock(index);
    }

    private bool IsMutuallyExclusive(TimelineBlockSO.BlockType type)
    {
        return type == TimelineBlockSO.BlockType.Mission || type == TimelineBlockSO.BlockType.Exploration || type == TimelineBlockSO.BlockType.DialogStandalone;
    }

    public int FindBlockIndexById(string id)
    {
        for (int i = 0; i < blockEntries.Count; i++)
        {
            if (blockEntries[i].block != null &&
                (blockEntries[i].block as TimelineBlockSO)?.blockId == id)
                return i;
        }
        return -1;
    }

    public void EnterExplorationFallback()
    {
        if (explorationFallbackIndex < 0 || explorationFallbackIndex >= blockEntries.Count)
        {
            Debug.LogWarning("[Timeline] No valid explorationFallbackIndex set.");
            return;
        }

        if (verboseLogging)
            Debug.Log("[Timeline] Entering fallback exploration mode.");

        JumpToBlock(explorationFallbackIndex);
    }

    private void HandleBlockCompleted(ITimelineBlock completedBlock)
    {
        if (completedBlock != currentBlock) return;

<<<<<<< Updated upstream
        OnBlockEnded?.Invoke(completedBlock);
=======
        // … existing teardown …
>>>>>>> Stashed changes

        // NEW: drain our queue first
        if (pendingRequests.Count > 0)
        {
            int nextQueued = pendingRequests.Dequeue();
            Debug.Log($"[Timeline] Launching queued block idx={nextQueued} → {blockEntries[nextQueued].block.name}");
            ScheduleBlock(nextQueued);
            return;
        }

        // FALLBACK: original branching logic
        int nextIndex = currentIndex + 1;
        int overrideIdx = blockEntries[currentIndex].overrideNextIndex;
        if (overrideIdx >= 0) nextIndex = overrideIdx;
        if (nextIndex >= 0 && nextIndex < blockEntries.Count)
            ScheduleBlock(nextIndex);
        else
        {
            Debug.Log("[Timeline] Reached end of sequence.");
            gpsUI?.Hide();
            currentBlock = null;
        }
    }

    #endregion

    #region Cleanup Helpers

    /// <summary>
    /// Stops and cleans up the currently running block (if any).
    /// Does NOT fire OnBlockEnded.
    /// </summary>
    private void CleanupCurrentBlock()
    {
        if (currentBlock != null)
        {
            if (verboseLogging) Debug.Log($"[Timeline] Cleaning up running block idx={currentIndex}");
            currentBlock.OnBlockCompleted -= HandleBlockCompleted;
            currentBlock.StopBlock();
            currentBlock = null;
        }
    }

    /// <summary>
    /// Stops any in‐flight entryDelay coroutine and cleans up current block.
    /// </summary>
    private void CleanupCurrentAndDelay()
    {
        // Stop any pending delay coroutine
        if (enteryDelayCoroutine != null)
        {
            StopCoroutine(enteryDelayCoroutine);
            enteryDelayCoroutine = null;
        }

        // Stop and clean up the active block
        CleanupCurrentBlock();

        // Ensure we’re not left in a paused state
        isPaused = false;
    }

    private void ResetAllBlocks()
    {
        foreach (var entry in blockEntries)
            entry.block?.ResetBlock();

        pendingRequests.Clear(); // also clear the “waiting room”
        if (verboseLogging) Debug.Log("[Timeline] All blocks reset.");
    }

    #endregion


}
