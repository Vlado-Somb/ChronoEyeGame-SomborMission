using UnityEditor;
using UnityEngine;
using MyGame.Timeline;

[CustomPropertyDrawer(typeof(MasterTimelineManager.BlockEntry))]
public class BlockEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        // Sub-properties
        var blockProp = prop.FindPropertyRelative("block");
        var delayProp = prop.FindPropertyRelative("entryDelay");
        var overrideProp = prop.FindPropertyRelative("overrideNextIndex");

        // Layout
        float h = EditorGUIUtility.singleLineHeight;
        Rect r1 = new Rect(pos.x, pos.y, pos.width, h);
        Rect r2 = new Rect(pos.x, pos.y + h + 2, pos.width, h);
        Rect r3 = new Rect(pos.x, pos.y + 2 * (h + 2), pos.width, h);

        EditorGUI.PropertyField(r1, blockProp);
        EditorGUI.PropertyField(r2, delayProp);

        // Determine the path to the containing array
        string path = prop.propertyPath; // e.g. "blockEntries.Array.data[3]"
        int arrayToken = path.IndexOf(".Array");
        string arrayPath = arrayToken >= 0
            ? path.Substring(0, arrayToken)
            : path; // fallback

        var arrayProp = prop.serializedObject.FindProperty(arrayPath);
        int count = arrayProp != null ? arrayProp.arraySize : 0;

        // Build dropdown options: <none> + each block name
        string[] labels;
        int[] values;
        if (arrayProp != null && count > 0)
        {
            labels = new string[count + 1];
            values = new int[count + 1];
            labels[0] = "<none>";
            values[0] = -1;
            for (int i = 0; i < count; i++)
            {
                var entry = arrayProp.GetArrayElementAtIndex(i);
                var bProp = entry.FindPropertyRelative("block");
                var bObj = bProp.objectReferenceValue as TimelineBlockSO;
                labels[i + 1] = bObj != null ? bObj.name : "<none>";
                values[i + 1] = i;
            }
        }
        else
        {
            labels = new[] { "<none>" };
            values = new[] { -1 };
        }

        // Current selection
        int current = System.Array.IndexOf(values, overrideProp.intValue);
        if (current < 0) current = 0;

        // Draw popup
        int picked = EditorGUI.Popup(r3, "Override Next Index", current, labels);
        overrideProp.intValue = values[picked];
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        return h * 3 + 4;
    }
}
