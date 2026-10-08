using UnityEngine;

/// <summary>
/// Always rotates this GameObject to face the main camera.
/// Attach this to your world-space Canvas so it constantly faces the AR camera.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCam == null) return;
        transform.forward = Camera.main.transform.forward;
    }
}
