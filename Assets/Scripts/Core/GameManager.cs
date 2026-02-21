using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImpressMyGuests.Core
{
    /// <summary>
    /// Central game manager — singleton that persists across scenes and coordinates
    /// high-level game state transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string characterCreationScene = "CharacterCreation";
        [SerializeField] private string homeDesignScene = "HomeDesign";
        [SerializeField] private string multiplayerLobbyScene = "MultiplayerLobby";

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void GoToMainMenu()
        {
            CurrentState = GameState.MainMenu;
            SceneLoader.LoadScene(mainMenuScene);
        }

        public void StartCharacterCreation()
        {
            CurrentState = GameState.CharacterCreation;
            SceneLoader.LoadScene(characterCreationScene);
        }

        public void StartHomeDesign()
        {
            CurrentState = GameState.HomeDesign;
            SceneLoader.LoadScene(homeDesignScene);
        }

        public void StartMultiplayer()
        {
            CurrentState = GameState.MultiplayerLobby;
            SceneLoader.LoadScene(multiplayerLobbyScene);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public enum GameState
    {
        MainMenu,
        CharacterCreation,
        HomeDesign,
        MultiplayerLobby,
        MultiplayerHomeDesign
    }
}
