#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TimelineDatabaseAutoFiller
{
    [MenuItem("Tools/Timeline/Autofill Active Block Database")] // changed menu name for clarity
    public static void Autofill()
    {
        var timelineManager = Object.FindFirstObjectByType<MasterTimelineManager>();
        if (timelineManager == null)
        {
            Debug.LogError("[Autofill] MasterTimelineManager not found in the scene.");
            return;
        }

        var db = timelineManager.ActiveDatabase;
        if (db == null)
        {
            Debug.LogError("[Autofill] No TimelineBlockDatabase linked in TimelineSettings.");
            return;
        }

        db.blocks.Clear();
        foreach (var entry in timelineManager.BlockEntries)
        {
            if (entry.block != null && !db.blocks.Contains(entry.block))
                db.blocks.Add(entry.block);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Autofill] TimelineBlockDatabase updated with {db.blocks.Count} entries.");
    }
}
#endif