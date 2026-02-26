using UnityEngine;

public class CollectibleSpawner2D : MonoBehaviour
{
    [Header("Pick from these prefabs")]
    [SerializeField] private GameObject[] collectiblePrefabs;

    [Header("Spawn behavior")]
    [Tooltip("If true, each spawn point picks a random prefab. If false, cycles through list.")]
    [SerializeField] private bool randomize = true;

    [Tooltip("If true, a prefab can be chosen multiple times across spawn points.")]
    [SerializeField] private bool allowRepeats = true;

    private void Start()
    {
        if (collectiblePrefabs == null || collectiblePrefabs.Length == 0)
        {
            Debug.LogWarning($"[{name}] No collectiblePrefabs assigned.");
            return;
        }

        // Collect child spawn points
        int childCount = transform.childCount;
        if (childCount == 0)
        {
            Debug.LogWarning($"[{name}] No child spawn points. Add empty children as spawn locations.");
            return;
        }

        bool[] used = allowRepeats ? null : new bool[collectiblePrefabs.Length];
        int usedCount = 0;
        int cycleIndex = 0;

        for (int i = 0; i < childCount; i++)
        {
            Transform point = transform.GetChild(i);
            if (point == null) continue;

            int prefabIndex;

            if (!randomize)
            {
                prefabIndex = cycleIndex % collectiblePrefabs.Length;
                cycleIndex++;
            }
            else
            {
                prefabIndex = PickIndex(used, ref usedCount);
                if (prefabIndex < 0) prefabIndex = Random.Range(0, collectiblePrefabs.Length); // fallback
            }

            GameObject prefab = collectiblePrefabs[prefabIndex];
            if (prefab == null) continue;

            Instantiate(prefab, point.position, point.rotation);
        }
    }

    private int PickIndex(bool[] used, ref int usedCount)
    {
        // If repeats allowed, simplest
        if (used == null)
            return Random.Range(0, collectiblePrefabs.Length);

        // No repeats: if we've used all, return -1
        if (usedCount >= used.Length)
            return -1;

        // Try a few random picks, then fallback to linear
        for (int tries = 0; tries < 20; tries++)
        {
            int idx = Random.Range(0, used.Length);
            if (!used[idx])
            {
                used[idx] = true;
                usedCount++;
                return idx;
            }
        }

        for (int idx = 0; idx < used.Length; idx++)
        {
            if (!used[idx])
            {
                used[idx] = true;
                usedCount++;
                return idx;
            }
        }

        return -1;
    }
}