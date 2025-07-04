using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Manual Player Assignment")]
    public GameObject player1GameObject;
    public GameObject player2GameObject;
    
    [Header("Round System")]
    public int maxRounds = 5; // Best of 5
    public float roundEndDelay = 3f; // Delay before next round
    public float gameEndDelay = 5f; // Delay before game restart
    
    [Header("Respawn Settings")]
    public float respawnDelay = 2f;
    public float respawnInvulnerabilityTime = 1f;
    public GameObject respawnEffectPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("UI References")]
    public GameObject roundWinUI; // UI to show round winner
    public GameObject gameWinUI; // UI to show game winner
    public UnityEngine.UI.Text roundWinText;
    public UnityEngine.UI.Text gameWinText;
    public UnityEngine.UI.Text scoreText; // Shows current score

    // Round tracking
    private int currentRound = 1;
    private int player1Wins = 0;
    private int player2Wins = 0;
    private bool gameInProgress = true;
    private bool roundInProgress = false;

    // Player tracking
    private List<GameObject> alivePlayers = new List<GameObject>();
    private Dictionary<GameObject, Vector3> playerOriginalSpawns = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, int> playerNumbers = new Dictionary<GameObject, int>();

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
        RegisterPlayers();
        StartNewRound();
        UpdateScoreUI();
    }

    void RegisterPlayers()
    {
        // Use manual assignment if available
        if (player1GameObject != null && player2GameObject != null)
        {
            // Clear existing data
            alivePlayers.Clear();
            playerNumbers.Clear();
            playerOriginalSpawns.Clear();

            // Register Player 1
            alivePlayers.Add(player1GameObject);
            playerNumbers[player1GameObject] = 1;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                playerOriginalSpawns[player1GameObject] = spawnPoints[0].position;
            }
            else
            {
                playerOriginalSpawns[player1GameObject] = player1GameObject.transform.position;
            }

            // Register Player 2
            alivePlayers.Add(player2GameObject);
            playerNumbers[player2GameObject] = 2;
            if (spawnPoints != null && spawnPoints.Length > 1)
            {
                playerOriginalSpawns[player2GameObject] = spawnPoints[1].position;
            }
            else
            {
                playerOriginalSpawns[player2GameObject] = player2GameObject.transform.position;
            }

            Debug.Log($"Registered {player1GameObject.name} as Player 1");
            Debug.Log($"Registered {player2GameObject.name} as Player 2");
        }
        else
        {
            // Fallback to automatic detection (without sorting)
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < players.Length; i++)
            {
                GameObject player = players[i];
                alivePlayers.Add(player);

                // Try to extract player number from name first
                int playerNumber = ExtractPlayerNumberFromName(player.name);
                if (playerNumber == -1)
                {
                    // Fallback to array index + 1
                    playerNumber = i + 1;
                }

                playerNumbers[player] = playerNumber;

                Debug.Log($"Registered {player.name} as Player {playerNumber}");

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
    }

    // Helper method to extract player number from GameObject name
    private int ExtractPlayerNumberFromName(string name)
    {
        // Look for patterns like "Player1", "Player 1", "Player_1", etc.
        if (name.ToLower().Contains("player"))
        {
            // Extract the number from the name
            string numberPart = "";
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsDigit(name[i]))
                {
                    numberPart += name[i];
                }
            }
            
            if (int.TryParse(numberPart, out int number))
            {
                return number;
            }
        }
        
        return -1; // Return -1 if no number found
    }

    void StartNewRound()
    {
        if (!gameInProgress) return;

        roundInProgress = true;
        
        // Reset all players to spawn positions
        foreach (GameObject player in playerNumbers.Keys)
        {
            ResetPlayerForRound(player);
        }

        // Rebuild alive players list
        alivePlayers.Clear();
        foreach (GameObject player in playerNumbers.Keys)
        {
            alivePlayers.Add(player);
        }

        Debug.Log($"Round {currentRound} started!");
        
        // Hide any UI panels
        if (roundWinUI != null) roundWinUI.SetActive(false);
        if (gameWinUI != null) gameWinUI.SetActive(false);
    }

    void ResetPlayerForRound(GameObject player)
    {
        // Reset position
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

        // Make sure player is active
        player.SetActive(true);
    }

    public void PlayerFell(GameObject player)
    {
        if (!roundInProgress || !alivePlayers.Contains(player)) return;

        alivePlayers.Remove(player);
        Debug.Log($"{player.name} fell! Alive players: {alivePlayers.Count}");

        // Check if round is over
        if (alivePlayers.Count <= 1)
        {
            EndRound();
        }
    }

    void EndRound()
    {
        if (!roundInProgress) return;
        
        roundInProgress = false;
        GameObject winner = null;

        // Determine round winner
        if (alivePlayers.Count == 1)
        {
            winner = alivePlayers[0];
            Debug.Log($"Winner found: {winner.name}");
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("Round ended in a draw!");
            // In case of draw, restart the same round
            StartCoroutine(RestartRoundAfterDelay());
            return;
        }

        if (winner != null)
        {
            int winnerNumber = playerNumbers[winner];
            Debug.Log($"Winner {winner.name} is assigned number: {winnerNumber}");
            
            // Award point to winner
            if (winnerNumber == 1)
            {
                player1Wins++;
            }
            else if (winnerNumber == 2)
            {
                player2Wins++;
            }

            Debug.Log($"Player {winnerNumber} wins round {currentRound}!");
            
            // Show round win UI
            ShowRoundWinUI(winnerNumber);
            
            // Update score display
            UpdateScoreUI();
            
            // Check if game is over
            if (IsGameOver())
            {
                EndGame();
            }
            else
            {
                StartCoroutine(StartNextRoundAfterDelay());
            }
        }
    }

    void ShowRoundWinUI(int winnerNumber)
    {
        if (roundWinUI != null && roundWinText != null)
        {
            roundWinUI.SetActive(true);
            roundWinText.text = $"Player {winnerNumber} Wins Round {currentRound}!";
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Player 1: {player1Wins} | Player 2: {player2Wins}";
        }
    }

    IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundEndDelay);
        
        currentRound++;
        StartNewRound();
    }

    IEnumerator RestartRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundEndDelay);
        StartNewRound();
    }

    bool IsGameOver()
    {
        int winsNeeded = Mathf.CeilToInt(maxRounds / 2f); // For best of 5, need 3 wins
        return player1Wins >= winsNeeded || player2Wins >= winsNeeded;
    }

    void EndGame()
    {
        gameInProgress = false;
        int gameWinner = player1Wins > player2Wins ? 1 : 2;
        
        Debug.Log($"Game Over! Player {gameWinner} wins the match!");
        
        // Show game win UI
        if (gameWinUI != null && gameWinText != null)
        {
            gameWinUI.SetActive(true);
            gameWinText.text = $"Player {gameWinner} Wins the Match!\n" +
                              $"Final Score: {player1Wins} - {player2Wins}";
        }
        
        // Hide round win UI
        if (roundWinUI != null) roundWinUI.SetActive(false);
        
        StartCoroutine(RestartGameAfterDelay());
    }

    IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSeconds(gameEndDelay);
        RestartGame();
    }

    void RestartGame()
    {
        // Reset all game state
        currentRound = 1;
        player1Wins = 0;
        player2Wins = 0;
        gameInProgress = true;
        
        // Hide UI
        if (roundWinUI != null) roundWinUI.SetActive(false);
        if (gameWinUI != null) gameWinUI.SetActive(false);
        
        // Start fresh
        StartNewRound();
        UpdateScoreUI();
        
        Debug.Log("Game restarted!");
    }

    // Keep existing methods for respawn system (in case you want mid-round respawns)
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
        if (playerOriginalSpawns.ContainsKey(player))
        {
            return playerOriginalSpawns[player];
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        return Vector3.zero;
    }

    private IEnumerator TemporaryInvulnerability(GameObject player)
    {
        PlayerPush playerPush = player.GetComponent<PlayerPush>();
        if (playerPush != null)
        {
            playerPush.SetInvulnerable(true);
        }

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
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    // Public methods for external access
    public int GetCurrentRound() => currentRound;
    public int GetPlayer1Wins() => player1Wins;
    public int GetPlayer2Wins() => player2Wins;
    public bool IsRoundInProgress() => roundInProgress;
    public int GetAlivePlayerCount() => alivePlayers.Count;
    
    // Manual restart method (can be called from UI button)
    public void ManualRestartGame()
    {
        RestartGame();
    }
}