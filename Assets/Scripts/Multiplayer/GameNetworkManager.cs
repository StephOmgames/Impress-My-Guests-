using Unity.Netcode;
using UnityEngine;
using ImpressMyGuests.CharacterCreation;

namespace ImpressMyGuests.Multiplayer
{
    /// <summary>
    /// Extends Unity Netcode's NetworkManager with game-specific lobby logic.
    /// Attach to the same GameObject as Unity's built-in NetworkManager component.
    /// </summary>
    public class GameNetworkManager : MonoBehaviour
    {
        public static GameNetworkManager Instance { get; private set; }

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 8;

        private NetworkManager _netManager;

        public bool IsHost    => _netManager != null && _netManager.IsHost;
        public bool IsClient  => _netManager != null && _netManager.IsClient;
        public bool IsServer  => _netManager != null && _netManager.IsServer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _netManager = GetComponent<NetworkManager>();
            if (_netManager == null)
                Debug.LogError("[GameNetworkManager] No NetworkManager component found on this GameObject.");
        }

        private void OnEnable()
        {
            if (_netManager == null) return;
            _netManager.OnClientConnectedCallback    += OnClientConnected;
            _netManager.OnClientDisconnectCallback   += OnClientDisconnected;
            _netManager.OnServerStarted              += OnServerStarted;
        }

        private void OnDisable()
        {
            if (_netManager == null) return;
            _netManager.OnClientConnectedCallback    -= OnClientConnected;
            _netManager.OnClientDisconnectCallback   -= OnClientDisconnected;
            _netManager.OnServerStarted              -= OnServerStarted;
        }

        // ── Session Control ─────────────────────────────────────────────────────

        /// <summary>Starts this client as a host (server + client).</summary>
        public void StartHost()
        {
            _netManager.StartHost();
        }

        /// <summary>Starts a dedicated server.</summary>
        public void StartServer()
        {
            _netManager.StartServer();
        }

        /// <summary>Connects to an existing session as a client.</summary>
        public void StartClient()
        {
            _netManager.StartClient();
        }

        /// <summary>Disconnects from the current session.</summary>
        public void Disconnect()
        {
            _netManager.Shutdown();
        }

        // ── Callbacks ───────────────────────────────────────────────────────────

        private void OnServerStarted()
        {
            Debug.Log("[GameNetworkManager] Server started.");
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[GameNetworkManager] Client connected: {clientId}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[GameNetworkManager] Client disconnected: {clientId}");
        }
    }
}
