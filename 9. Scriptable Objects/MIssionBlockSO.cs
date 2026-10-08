using System;
using System.Collections;
using UnityEngine;
using MyGame.Timeline;

[CreateAssetMenu(menuName = "Timeline/Mission Block")]
public class MissionBlockSO : TimelineBlockSO
{
    [Tooltip("This mission will be assigned to MissionManager and started.")]
    public MissionSO missionAsset;

    public static event Action<MissionSO> OnMissionBlockAboutToFire;

    public override void StartBlock()
    {
        Debug.Log($"[MissionBlockSO] StartBlock() called on: {name} (ID: {blockId})");

        if (missionAsset == null)
        {
            Debug.LogError("[MissionBlockSO] MissionAsset is null!");
            CompleteBlock();
            return;
        }

        manager.StartCoroutine(WaitForMissionManager());
    }

    private IEnumerator WaitForMissionManager()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (MissionManager.Instance == null && elapsed < timeout)
        {
            Debug.Log("[MissionBlockSO] Waiting for MissionManager.Instance...");
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (MissionManager.Instance == null)
        {
            Debug.LogError("[MissionBlockSO] MissionManager.Instance still null after timeout.");
            CompleteBlock();
            yield break;
        }

        if (verbose)
            Debug.Log($"[MissionBlock] Starting mission: {missionAsset.missionName}");

        MissionManager.Instance.OnMissionComplete += HandleMissionComplete;
        OnMissionBlockAboutToFire?.Invoke(missionAsset);
        MissionManager.Instance.BeginMission(missionAsset);
    }

    private void HandleMissionComplete()
    {
        MissionManager.Instance.OnMissionComplete -= HandleMissionComplete;
        if (verbose) Debug.Log("[MissionBlock] Mission completed. Advancing timeline.");
        CompleteBlock();
    }

    public override void StopBlock()
    {
        MissionManager.Instance.OnMissionComplete -= HandleMissionComplete;
    }

    public override void ResetBlock()
    {
        // MissionManager handles its own reset per mission load
    }

    private bool verbose => manager != null && manager.verboseLogging;
}
