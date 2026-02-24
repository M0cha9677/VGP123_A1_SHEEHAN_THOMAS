using UnityEngine;
//using Unity.SceneManagement; 

public class LevelStart : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Spawner missing references.");
            return;
        }

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        CameraFollow2D cam = FindFirstObjectByType<CameraFollow2D>();
        if (cam != null)
            cam.SetTarget(player.transform);
        else
            Debug.LogWarning("No CameraFollow found in scene");

    }
}
