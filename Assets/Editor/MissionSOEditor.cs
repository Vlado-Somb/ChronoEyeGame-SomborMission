using UnityEngine;

using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MissionSO))]
public class MissionSOEditor : Editor
{
    private SerializedProperty _waypointDb;
    private SerializedProperty _tasks;
    private bool[] _taskFoldouts = new bool[0];

    private void OnEnable()
    {
        _waypointDb = serializedObject.FindProperty("waypointDatabase");
        _tasks = serializedObject.FindProperty("tasks");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("missionName"));
        EditorGUILayout.PropertyField(_waypointDb);

        // Resize foldouts array if tasks count changed
        if (_taskFoldouts.Length != _tasks.arraySize)
            _taskFoldouts = new bool[_tasks.arraySize];

        for (int i = 0; i < _tasks.arraySize; i++)
        {
            var element = _tasks.GetArrayElementAtIndex(i);
            var typeProp = element.FindPropertyRelative("taskType");
            string header = $"Task {i} : {typeProp.enumNames[typeProp.enumValueIndex]}";

            _taskFoldouts[i] = EditorGUILayout.Foldout(_taskFoldouts[i], header, true);
            if (!_taskFoldouts[i]) continue;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(typeProp);


            EditorGUILayout.PropertyField(element.FindPropertyRelative("title"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("instruction"));

            // Conditional fields
            switch ((MissionTask.MissionTaskType)typeProp.enumValueIndex)
            {
                case MissionTask.MissionTaskType.GPS:
                    DrawGpsTask(element);
                    break;
                case MissionTask.MissionTaskType.POI:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("poiPrefab"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("poiIdentifier"));
                    break;
                case MissionTask.MissionTaskType.Question:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("questionText"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("Choices"), true);
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("CorrectChoiceIndex"));
                    break;
                case MissionTask.MissionTaskType.TextInput:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("inputPlaceholder"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("answerPlaceholder"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("expectedAnswer"));
                    break;
                case MissionTask.MissionTaskType.Custom:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("customData"));
                    break;
            }

            EditorGUILayout.PropertyField(element.FindPropertyRelative("completionTitle"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("completionQuote"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("taskCompletionDelay"), new GUIContent("Post-Completion Delay (sec)"));


            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        // ✅ MOVE buttons outside the GPS block:
        if (GUILayout.Button("+ Add New Task"))
        {
            _tasks.arraySize++;
        }

        if (_tasks.arraySize > 0 && GUILayout.Button("− Remove Last Task"))
        {
            _tasks.arraySize--;
        }

        serializedObject.ApplyModifiedProperties();
    }


    private void DrawGpsTask(SerializedProperty element)
    {
        var waypointIdProp = element.FindPropertyRelative("waypointId");
        var confirmationZoneProp = element.FindPropertyRelative("confirmationZone");

        var waypointDbProp = serializedObject.FindProperty("waypointDatabase");
        WaypointDatabase db = null;
        if (waypointDbProp != null && waypointDbProp.objectReferenceValue != null)
            db = waypointDbProp.objectReferenceValue as WaypointDatabase;

        if (db == null || db.waypoints == null || db.waypoints.Count == 0)
        {
            EditorGUILayout.HelpBox("WaypointDatabase is not assigned or contains no waypoints.", MessageType.Warning);
            return;
        }

        string[] options = db.waypoints.Select(w => $"{w.displayName} [{w.id}]").ToArray();
        int currentIndex = Mathf.Max(0, db.waypoints.FindIndex(w => w.id == waypointIdProp.stringValue));

        int newIndex = EditorGUILayout.Popup("Target Waypoint", currentIndex, options);
        if (newIndex >= 0 && newIndex < db.waypoints.Count)
            waypointIdProp.stringValue = db.waypoints[newIndex].id;

        EditorGUILayout.PropertyField(confirmationZoneProp, new GUIContent("Required Zone to Complete"));

    }




}
