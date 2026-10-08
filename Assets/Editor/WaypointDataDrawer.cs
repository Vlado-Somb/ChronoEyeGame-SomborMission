using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WaypointData))]
public class WaypointDataDrawer : PropertyDrawer
{
    private bool _showTrigger = true;
    private bool _showDelays = true;
    private bool _showGeo = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty id = property.FindPropertyRelative("id");
        SerializedProperty displayName = property.FindPropertyRelative("displayName");
        SerializedProperty type = property.FindPropertyRelative("type");

        SerializedProperty latitude = property.FindPropertyRelative("latitude");
        SerializedProperty longitude = property.FindPropertyRelative("longitude");
        SerializedProperty altitude = property.FindPropertyRelative("altitude");

        SerializedProperty radius1 = property.FindPropertyRelative("triggerRadiusLevel1");
        SerializedProperty radius2 = property.FindPropertyRelative("triggerRadiusLevel2");
        SerializedProperty radius3 = property.FindPropertyRelative("triggerRadiusLevel3");

        SerializedProperty delay1 = property.FindPropertyRelative("zone1ConfirmDelay");
        SerializedProperty delay2 = property.FindPropertyRelative("zone2ConfirmDelay");
        SerializedProperty delay3 = property.FindPropertyRelative("zone3ConfirmDelay");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, id.stringValue, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = foldoutRect.y + EditorGUIUtility.singleLineHeight + 2f;


            // 🗺 Geolocation
            _showGeo = EditorGUI.Foldout(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), _showGeo, "Geolocation", true);
            y += EditorGUIUtility.singleLineHeight;
            if (_showGeo)
            {
                id.stringValue = EditorGUI.TextField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "ID", id.stringValue);
                y += EditorGUIUtility.singleLineHeight;

                type.enumValueIndex = (int)(WaypointData.WaypointType)EditorGUI.EnumPopup(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight),"Waypoint Type", (WaypointData.WaypointType)type.enumValueIndex);
                y += EditorGUIUtility.singleLineHeight;

                displayName.stringValue = EditorGUI.TextField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight),"Display Name", displayName.stringValue);
                y += EditorGUIUtility.singleLineHeight;

                latitude.doubleValue = EditorGUI.DoubleField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Latitude", latitude.doubleValue);
                y += EditorGUIUtility.singleLineHeight;

                longitude.doubleValue = EditorGUI.DoubleField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Longitude", longitude.doubleValue);
                y += EditorGUIUtility.singleLineHeight;

                altitude.doubleValue = EditorGUI.DoubleField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Altitude", altitude.doubleValue);
                y += EditorGUIUtility.singleLineHeight;

                // 🔵 Trigger Radii
                _showTrigger = EditorGUI.Foldout(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), _showTrigger, "Trigger Radius (m)", true);
                y += EditorGUIUtility.singleLineHeight;
                if (_showTrigger)
                {
                    radius1.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Level 1 Radius", radius1.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                    radius2.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Level 2 Radius", radius2.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                    radius3.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Level 3 Radius", radius3.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                }

                // 🟡 Zone Entry Delays
                _showDelays = EditorGUI.Foldout(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), _showDelays, "Zone Entry Delays (s)", true);
                y += EditorGUIUtility.singleLineHeight;
                if (_showDelays)
                {
                    delay1.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Zone 1 Delay", delay1.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                    delay2.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Zone 2 Delay", delay2.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                    delay3.floatValue = EditorGUI.FloatField(new Rect(position.x + 16, y, position.width - 16, EditorGUIUtility.singleLineHeight), "Zone 3 Delay", delay3.floatValue);
                    y += EditorGUIUtility.singleLineHeight;
                }

            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded) return height;

        height += EditorGUIUtility.singleLineHeight * 3; // For the foldouts

        if (_showTrigger) height += EditorGUIUtility.singleLineHeight * 3;
        if (_showDelays) height += EditorGUIUtility.singleLineHeight * 3;
        if (_showGeo) height += EditorGUIUtility.singleLineHeight * 6;

        return height;
    }
}
