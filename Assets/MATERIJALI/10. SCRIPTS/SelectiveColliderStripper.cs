using UnityEngine;

public class SelectiveColliderStripper : MonoBehaviour
{
    public float delay = 1.5f;
    public float triangleSizeThreshold = 500f; // Optional: Not directly accessible without mesh analysis
    public string[] ignoreTags = { "POI", "Interactive" };
    public bool destroyInsteadOfDisable = true;

    private void Start()
    {
        InvokeRepeating(nameof(StripUnwantedColliders), delay, 2f);
    }

    void StripUnwantedColliders()
    {
        MeshCollider[] colliders = Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);
        int stripped = 0;

        foreach (var col in colliders)
        {
            GameObject obj = col.gameObject;
            bool isIgnored = false;

            foreach (string tag in ignoreTags)
            {
                if (obj.CompareTag(tag))
                {
                    isIgnored = true;
                    break;
                }
            }

            if (!isIgnored && (obj.name.StartsWith("Tile") || obj.name.Contains("Cesium")))
            {
                if (destroyInsteadOfDisable)
                    Destroy(col);
                else
                    col.enabled = false;

                stripped++;
            }
        }

        if (stripped > 0)
        {
            Debug.Log($"[SelectiveColliderStripper] Removed or disabled {stripped} MeshColliders.");
        }
    }
}
