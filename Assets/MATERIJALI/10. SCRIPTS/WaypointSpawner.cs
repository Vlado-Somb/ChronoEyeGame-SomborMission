using UnityEngine;
using UnityEngine.UIElements;

public class WaypointSpawner
{
    private readonly VisualTreeAsset _waypointTemplate;
    private readonly VisualElement _container;

    public WaypointSpawner(VisualTreeAsset template, VisualElement container)
    {
        _waypointTemplate = template;
        _container = container;
    }

    // WaypointSpawner.cs
    public VisualElement SpawnWaypoint(string waypointId, string labelText, Vector2 absolutePosition)
    {
        var instance = _waypointTemplate.CloneTree();

        var label = instance.Q<Label>("WaypointLabel");
        if (label != null) label.text = labelText;

        var wrapper = instance.Q<VisualElement>("WaypointWrapper");
        if (wrapper != null)
        {
            wrapper.name = $"WaypointWrapper_{waypointId}";
            wrapper.style.position = Position.Absolute;
            wrapper.style.left = absolutePosition.x;
            wrapper.style.top = absolutePosition.y;

            // Center the marker on its point after layout
            wrapper.schedule.Execute(() =>
            {
                float offsetX = wrapper.resolvedStyle.width / 2f;
                float offsetY = wrapper.resolvedStyle.height; // anchor at bottom center
                wrapper.style.left = absolutePosition.x - offsetX;
                wrapper.style.top = absolutePosition.y - offsetY;
            }).ExecuteLater(30);
        }

        // IMPORTANT: add the *wrapper* (not the template root) to the container
        _container.Add(wrapper);

        Debug.Log($"[WaypointSpawner] Spawned waypoint '{waypointId}' ({labelText}) at {absolutePosition}");
        return wrapper;
    }

}
