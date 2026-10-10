using UnityEditor;
using UnityEngine;
using MyGame.Waypoint;
using MyGame.Timeline;

[CustomEditor(typeof(WaypointTriggerBinding))]
public class WaypointTriggerBindingAssetEditor : Editor
{
    private SerializedProperty _bindingsProp;
    private WaypointTriggerBinding _asset;

    private void OnEnable()
    {
        _asset = (WaypointTriggerBinding)target;
        _bindingsProp = serializedObject.FindProperty("waypointTriggers");

        // Register context for drawer
        WaypointEditorContext.ActiveSettings = _asset.waypointSettings;

        var config = AssetDatabase.LoadAssetAtPath<GlobalTimelineConfig>(
            "Assets/MATERIJALI/11. Upoznaj Sombor ASSETS/Global Timeline Config.asset"
        );
        WaypointEditorContext.ActiveTimelineSettings = _asset.timelineSettings ?? config?.GetActiveTimelineSettings();

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(4);

        // Settings fields
        EditorGUI.BeginChangeCheck();
        _asset.waypointSettings = (WaypointSettings)EditorGUILayout.ObjectField(
            "Waypoint Settings",
            _asset.waypointSettings,
            typeof(WaypointSettings),
            false
        );
        _asset.timelineSettings = (TimelineSettings)EditorGUILayout.ObjectField(
            "Timeline Settings",
            _asset.timelineSettings,
            typeof(TimelineSettings),
            false
        );
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_asset);
            WaypointEditorContext.ActiveSettings = _asset.waypointSettings;
            WaypointEditorContext.ActiveTimelineSettings = _asset.timelineSettings;
        }

        if (_asset.waypointSettings == null || _asset.timelineSettings == null)
        {
            EditorGUILayout.HelpBox("❌ Assign both WaypointSettings and TimelineSettings to enable editing.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Waypoint Triggers", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        for (int i = 0; i < _bindingsProp.arraySize; i++)
        {
            SerializedProperty element = _bindingsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            // Foldout with label (Binding i)
            SerializedProperty groupIdProp = element.FindPropertyRelative("groupId");
            SerializedProperty labelProp = element.FindPropertyRelative("bindingLabel");

            string header = !string.IsNullOrEmpty(labelProp.stringValue)
                ? labelProp.stringValue
                : $"Binding {i + 1}";

            groupIdProp.isExpanded = EditorGUILayout.Foldout(groupIdProp.isExpanded, header, true);



            if (groupIdProp.isExpanded)
            {
                EditorGUILayout.PropertyField(element);
            }

            // Delete Button
            EditorGUILayout.Space(2);
            if (GUILayout.Button("× Delete Binding"))
            {
                _bindingsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }

        EditorGUI.indentLevel--;

        // Add Button
        if (GUILayout.Button("+ Add New Binding"))
        {
            _bindingsProp.InsertArrayElementAtIndex(_bindingsProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
