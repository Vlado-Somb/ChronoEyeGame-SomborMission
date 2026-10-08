#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MasterTimelineManager))]
public class MasterTimelineManagerReorderable : Editor
{
    private ReorderableList _list;

    void OnEnable()
    {
        var so = serializedObject;
        var prop = so.FindProperty("blockEntries");

        _list = new ReorderableList(
            so, prop,
            draggable: true,
            displayHeader: true,
            displayAddButton: true,
            displayRemoveButton: true);

        _list.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Timeline Sequence", EditorStyles.boldLabel);

        _list.elementHeightCallback = i =>
            EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i), true);

        _list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = prop.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _list.DoLayoutList();

        // Draw the rest of the master settings
        DrawPropertiesExcluding(serializedObject, "blockEntries");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
