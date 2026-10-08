using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;

[RequireComponent(typeof(ARCoreExtensions))]
public class ARCoreFixer : MonoBehaviour
{
    [SerializeField] private ARSession session;

    private void Awake()
    {
        var extensions = GetComponent<ARCoreExtensions>();
        if (extensions != null && session != null)
        {
            Debug.Log("[ARCoreFixer] Forcing ARSession reference at runtime.");
            typeof(ARCoreExtensions)
                .GetField("_session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(extensions, session);
        }
    }
}
