//2 TriggerConditionListDrawer.cs
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using MyGame.Timeline;

[CustomEditor(typeof(TimelineBlockSO))]
public class TriggerConditionListDrawer : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        SerializedProperty triggerConditionsProp = serializedObject.FindProperty("triggerConditions");

        list = new ReorderableList(serializedObject, triggerConditionsProp, true, true, true, true);

        list.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Trigger Conditions");
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) => {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        };

        list.elementHeightCallback = index =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 8;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private string GetSummary(SerializedProperty element)
    {
        TriggerType type = (TriggerType)element.FindPropertyRelative("triggerType").enumValueIndex;
        switch (type)
        {
            case TriggerType.Waypoint:
                return "Location: " + element.FindPropertyRelative("waypointID").stringValue;
            case TriggerType.Time:
                return "Time: " + element.FindPropertyRelative("delaySeconds").floatValue + "s";
            case TriggerType.Dialog:
                return "Dialog: " + element.FindPropertyRelative("dialogID").stringValue;
            case TriggerType.POI:
                return "POI: " + element.FindPropertyRelative("poiID").stringValue;
            default:
                return "Unknown";
        }
    }
}
