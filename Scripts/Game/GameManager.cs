using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Initialize overall game settings
        Debug.Log("Game Initialized");
        // Load initial game state, setup UI, etc.
    }

    public void ChangeGameState(string newState)
    {
        // Handle state transitions
        Debug.Log($"Game State changed to: {newState}");
    }

    public void SaveGameState()
    {
        // Code to save game data
    }

    public void LoadGameState()
    {
        // Code to load game data
    }
}