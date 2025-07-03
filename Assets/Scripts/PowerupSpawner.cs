using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [Header("Powerup Settings")]
    public GameObject[] powerupPrefabs;
    public float arenaWidth = 10f;
    public float arenaLength = 10f;

    [Header("Spawn Timing")]
    public float delayBetweenSpawns = 7f; // Total time between powerups

    [Header("Spawn Placement")]
    public Transform arenaFloor; // Your arena floor object
    public float yOffset = 0.01f; // Offset above the floor

    private GameObject currentPowerup;

    void Start()
    {
        StartCoroutine(SpawnPowerupRoutine());
    }

    System.Collections.IEnumerator SpawnPowerupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBetweenSpawns);

            if (powerupPrefabs.Length == 0 || currentPowerup != null) continue;

            Vector3 spawnPos = GetSpawnPositionOnArena();

            GameObject prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];
            currentPowerup = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Destroy after 5 seconds
            Destroy(currentPowerup, 5f);

            // Wait for its life to end
            yield return new WaitForSeconds(5f);
            currentPowerup = null;
        }
    }

    Vector3 GetSpawnPositionOnArena()
    {
        float x = Random.Range(-arenaWidth / 2f, arenaWidth / 2f);
        float z = Random.Range(-arenaLength / 2f, arenaLength / 2f);
        float y = arenaFloor != null ? arenaFloor.position.y + yOffset : yOffset;

        return new Vector3(x, y, z);
    }
}
