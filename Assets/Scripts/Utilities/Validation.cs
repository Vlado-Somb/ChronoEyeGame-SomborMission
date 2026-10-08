using UnityEngine;
using UnityEngine.UIElements;

namespace UITKUtils
{
    public class Validation
    {
        public static void CheckQuery(VisualElement element, string name)
        {
            if (element == null)
                Debug.LogWarning($"Missing element named {name}");
        }
    }
}