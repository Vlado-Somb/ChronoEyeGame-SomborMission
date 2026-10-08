#if UNITY_EDITOR
// TriggerConditionDrawer.cs — refactored to use WaypointCache
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using MyGame.Waypoint;
using MyGame.Timeline;


[CustomPropertyDrawer(typeof(TimelineTriggerCondition))]
public class TriggerConditionDrawer : PropertyDrawer
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    static string[] _tc_WaypointIds;
#pragma warning restore UDR0001 // Domain Reload Analyzer
#pragma warning disable UDR0001 // Domain Reload Analyzer
    static GUIContent[] _tc_WaypointLabels;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null || !property.isArray && property.propertyType != SerializedPropertyType.Generic)
        {
            EditorGUI.LabelField(position, label.text, "Invalid trigger condition.");
            return;
        }
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        SerializedProperty typeProp = property.FindPropertyRelative("triggerType");
        SerializedProperty waypointProp = property.FindPropertyRelative("conWaypointId");
        SerializedProperty zoneProp = property.FindPropertyRelative("requiredZone");
        SerializedProperty delayProp = property.FindPropertyRelative("requiredSeconds");
        SerializedProperty dialogProp = property.FindPropertyRelative("dialogId");
        SerializedProperty poiProp = property.FindPropertyRelative("poiId");
        SerializedProperty uiIdProp = property.FindPropertyRelative("uiElementId");
        SerializedProperty missionNameProp = property.FindPropertyRelative("missionName");
        SerializedProperty blockIdProp = property.FindPropertyRelative("blockId");
        SerializedProperty imageNameProp = property.FindPropertyRelative("imageName");
        SerializedProperty subEventProp = property.FindPropertyRelative("subEventType");
        SerializedProperty labelProp = property.FindPropertyRelative("label");
        SerializedProperty onceProp = property.FindPropertyRelative("triggerOnlyOnce");
        SerializedProperty hasTrigProp = property.FindPropertyRelative("hasTriggered");

        Rect line = new Rect(position.x, y, position.width, lineHeight);
        EditorGUI.PropertyField(line, typeProp);
        y += lineHeight + spacing;

        TriggerType type = (TriggerType)typeProp.enumValueIndex;

        string[] blockLabels = Array.Empty<string>();
        string[] blockIds = Array.Empty<string>();

        var timelineDb = WaypointEditorContext.ActiveTimelineSettings?.timelineDatabase;
        if (timelineDb != null)
        {
            blockLabels = timelineDb.GetBlockLabels();
            blockIds = timelineDb.GetBlockIds();
        }

        switch (type)
        {
            case TriggerType.Time:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), delayProp);
                y += lineHeight + spacing;
                break;

            case TriggerType.Waypoint:
                {
                    // 1) pull from the centralized cache
                    var ids = WaypointCache.CachedWaypointIds;
                    var options = WaypointCache.CachedWaypointLabels;

                    // 2) compute oldIndex safely
                    int rawIndex = Array.IndexOf(ids, waypointProp.stringValue);
                    int oldIndex = Mathf.Clamp(rawIndex, 0, ids.Length - 1);

                    // 3) draw the popup (must use GUIContent for both label & options)
                    Rect popupRect = new Rect(position.x, y, position.width - 24, lineHeight);
                    GUIContent labelGUI = new GUIContent("Waypoint");
                    int newIndex = EditorGUI.Popup(popupRect, labelGUI,
                                                     oldIndex,
                                                     options.Length > 0
                                                     ? options
                                                     : new[] { new GUIContent("<no DB>") });

                    // 4) if changed, write back
                    if (newIndex != oldIndex && newIndex >= 0 && newIndex < ids.Length)
                        waypointProp.stringValue = ids[newIndex];


                    // 5) small refresh button
                    Rect btnRect = new Rect(position.x + position.width - 20, y, 20, lineHeight);
                    if (GUI.Button(btnRect, new GUIContent("⟳", "Refresh Waypoints")))
                        WaypointCache.RefreshWaypointCache();

                    y += lineHeight + spacing;

                    // 6) draw the rest of the fields
                    EditorGUI.PropertyField(
                        new Rect(position.x, y, position.width, lineHeight),
                        zoneProp
                    );
                    y += lineHeight + spacing;
                    break;
                }

            case TriggerType.Dialog:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), dialogProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                break;

            case TriggerType.POI:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), poiProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                break;

            case TriggerType.UI:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), uiIdProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                break;
            case TriggerType.Mission:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), missionNameProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                break;

            case TriggerType.GameBlock:
                {
                    int selectedIndex = Mathf.Max(0, Array.IndexOf(blockIds, blockIdProp.stringValue));

                    int newIndex = EditorGUI.Popup(
                        new Rect(position.x, y, position.width, lineHeight),
                        "Block Id",
                        selectedIndex,
                        blockLabels
                    );

                    if (newIndex != selectedIndex && newIndex >= 0 && newIndex < blockIds.Length)
                        blockIdProp.stringValue = blockIds[newIndex];

                    y += lineHeight + spacing;

                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                    y += lineHeight + spacing;

                    break;
                }



            case TriggerType.TrackedImage:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), imageNameProp);
                y += lineHeight + spacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                y += lineHeight + spacing;
                break;
        }

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), onceProp);
        y += lineHeight;
        GUI.enabled = false;

        // ─── Auto-generate conditionId ───────────────────────────
        var condIdProp = property.FindPropertyRelative("conditionId");
        if (condIdProp != null)
        {
            string genId = type switch
            {
                TriggerType.Waypoint => $"WP_{waypointProp.stringValue}_{zoneProp.enumValueIndex}",
                TriggerType.Time => $"TIME_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{delayProp.floatValue:F0}",
                TriggerType.Dialog => $"DIALOG_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{dialogProp.stringValue}",
                TriggerType.POI => $"POI_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{poiProp.stringValue}",
                TriggerType.UI => $"UI_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{uiIdProp.stringValue}",
                TriggerType.Mission => $"MISSION_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{missionNameProp.stringValue}",
                TriggerType.GameBlock => $"BLOCK_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{blockIdProp.stringValue}",
                TriggerType.TrackedImage => $"IMAGE_{subEventProp.enumNames[subEventProp.enumValueIndex]}_{imageNameProp.stringValue}",
                _ => $"NONE_{type}"
            };


            if (condIdProp.stringValue != genId)
            {
                condIdProp.stringValue = genId;
                // 👉 Persist back into the asset/scene
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        // ───────────────────────────────────────────────────────────

        // Auto-generate label for UI use
        if (labelProp != null && string.IsNullOrEmpty(labelProp.stringValue))
        {
            labelProp.stringValue = $"{type.ToString()}_auto";
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("triggerType");
        if (typeProp == null) return EditorGUIUtility.singleLineHeight;

        TriggerType type = (TriggerType)typeProp.enumValueIndex;
        int lines = 2; // base + hasTriggered + triggerOnce

        switch (type)
        {
            case TriggerType.Time: lines += 2; break;
            case TriggerType.Waypoint: lines += 2; break;
            case TriggerType.Dialog: lines += 2; break;
            case TriggerType.POI: lines += 2; break;
            case TriggerType.UI: lines += 2; break;
            case TriggerType.Mission: lines += 2; break;
            case TriggerType.GameBlock: lines += 2; break;
            case TriggerType.TrackedImage: lines += 2; break;
        }

        return lines * EditorGUIUtility.singleLineHeight + (lines) * 4f;
    }
}

/*

                if (subEventProp != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), subEventProp);
                    y += lineHeight + spacing;
                }
                if (delayProp != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), delayProp);
                    y += lineHeight + spacing;
                }

                }
                if (zoneProp != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), zoneProp);
                    y += lineHeight + spacing;
                }
                if (onceProp != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), onceProp);
                    y += lineHeight + spacing;
                }
*/
#endif