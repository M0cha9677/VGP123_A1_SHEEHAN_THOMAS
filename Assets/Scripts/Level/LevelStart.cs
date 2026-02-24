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

        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
    }
}
