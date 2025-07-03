using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// Enhanced GameManager to handle respawning
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respawn Settings")]
    public float respawnDelay = 2f;
    public float respawnInvulnerabilityTime = 1f;
    public GameObject respawnEffectPrefab; // Optional particle effect

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private List<GameObject> alivePlayers = new List<GameObject>();
    private Dictionary<GameObject, Vector3> playerOriginalSpawns = new Dictionary<GameObject, Vector3>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Register all players and their spawn points
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            alivePlayers.Add(player);

            // Store original spawn position or use spawn points
            if (spawnPoints != null && i < spawnPoints.Length)
            {
                playerOriginalSpawns[player] = spawnPoints[i].position;
            }
            else
            {
                playerOriginalSpawns[player] = player.transform.position;
            }
        }
    }

    public void PlayerFell(GameObject player)
    {
        if (alivePlayers.Contains(player))
        {
            alivePlayers.Remove(player);
            StartCoroutine(RespawnPlayer(player));
        }
    }

    private IEnumerator RespawnPlayer(GameObject player)
    {
        // Hide the player
        player.SetActive(false);

        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        // Reset player position and state
        Vector3 respawnPosition = GetRespawnPosition(player);
        player.transform.position = respawnPosition;
        player.transform.rotation = Quaternion.identity;

        // Reset physics
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset player controller state
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetPlayerState();
        }

        // Reactivate player
        player.SetActive(true);

        // Add back to alive players
        alivePlayers.Add(player);

        // Spawn effect
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, respawnPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Make player invulnerable temporarily
        StartCoroutine(TemporaryInvulnerability(player));

        Debug.Log($"{player.name} respawned!");
    }

    private Vector3 GetRespawnPosition(GameObject player)
    {
        // Try to use the stored original spawn position
        if (playerOriginalSpawns.ContainsKey(player))
        {
            return playerOriginalSpawns[player];
        }

        // Fallback to a random spawn point
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        // Ultimate fallback to origin
        return Vector3.zero;
    }

    private IEnumerator TemporaryInvulnerability(GameObject player)
    {
        PlayerPush playerPush = player.GetComponent<PlayerPush>();
        if (playerPush != null)
        {
            playerPush.SetInvulnerable(true);
        }

        // Visual feedback - make player flash
        StartCoroutine(FlashPlayer(player));

        yield return new WaitForSeconds(respawnInvulnerabilityTime);

        if (playerPush != null)
        {
            playerPush.SetInvulnerable(false);
        }
    }

    private IEnumerator FlashPlayer(GameObject player)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        float flashDuration = respawnInvulnerabilityTime;
        float flashInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            // Toggle visibility
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Ensure all renderers are visible at the end
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    public int GetAlivePlayerCount()
    {
        return alivePlayers.Count;
    }

    public bool IsGameOver()
    {
        return alivePlayers.Count <= 1;
    }
}
