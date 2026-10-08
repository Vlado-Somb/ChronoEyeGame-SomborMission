//2 TriggerSenderHelpers.cs
using UnityEngine;

public static class TriggerSenderHelpers
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static TriggerEvaluator _evaluator;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    private static TriggerEvaluator GetEvaluator()
    {
        if (_evaluator == null)
            _evaluator = GameObject.FindFirstObjectByType<TriggerEvaluator>();

        if (_evaluator == null)
            Debug.LogError("[TriggerSenderHelpers] No TriggerEvaluator found in scene!");

        return _evaluator;
    }

    public static void FireDialogTrigger(string dialogId, TriggerSubEventType subEvent)
    {
        var eval = GetEvaluator();
        if (eval != null)
            eval.TriggerByDialogEvent(dialogId, subEvent);
    }

    public static void FirePOITrigger(string poiId, TriggerSubEventType subEvent)
    {
        var eval = GetEvaluator();
        if (eval != null)
            eval.TriggerByPOIEvent(poiId, subEvent);
    }

    public static void FireUITrigger(string uiEventId, TriggerSubEventType subEvent)
    {
        var eval = GetEvaluator();
        if (eval != null)
            eval.TriggerByUIEvent(uiEventId, subEvent);
    }

    public static bool HasTriggerFired(TimelineTriggerCondition condition)
    {
        string key = condition.triggerType switch
        {
            TriggerType.POI => $"poi_{condition.poiId}_{condition.subEventType}",
            TriggerType.Dialog => $"dialog_{condition.dialogId}_{condition.subEventType}",
            TriggerType.UI => $"ui_{condition.uiElementId}_{condition.subEventType}",
            _ => null
        };

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"[TriggerSenderHelpers] ⚠️ Unsupported TriggerType for HasTriggerFired.");
            return false;
        }

        var eval = GetEvaluator();
        return eval != null && eval.HasFired(key);
    }

}
