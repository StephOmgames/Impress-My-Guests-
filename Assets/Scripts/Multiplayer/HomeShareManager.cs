using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using ImpressMyGuests.HomeDesign;

namespace ImpressMyGuests.Multiplayer
{
    /// <summary>
    /// Manages sharing of home-design states across the network.
    /// The host authoratively owns all home snapshots; clients request them via RPCs.
    /// </summary>
    public class HomeShareManager : NetworkBehaviour
    {
        public static HomeShareManager Instance { get; private set; }

        // Raised on all clients when a home snapshot is received.
        public UnityEvent<string, string> OnHomeSnapshotReceived = new UnityEvent<string, string>();

        // Server-side dictionary of clientId → JSON snapshot.
        private readonly Dictionary<ulong, string> _homeSnapshots = new Dictionary<ulong, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // ── Upload ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the local player when they finish (or auto-save) their home design.
        /// Sends the home snapshot JSON to the server.
        /// </summary>
        public void UploadHome(HomeSnapshot snapshot)
        {
            string json = JsonUtility.ToJson(snapshot);
            UploadHomeServerRpc(json);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UploadHomeServerRpc(string homeJson, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            _homeSnapshots[clientId] = homeJson;
            Debug.Log($"[HomeShareManager] Received home from client {clientId}.");

            // Notify all clients that this home is available.
            BroadcastHomeClientRpc(clientId, homeJson);
        }

        // ── Download ────────────────────────────────────────────────────────────

        /// <summary>Requests the home snapshot for a specific player from the server.</summary>
        public void RequestHome(ulong targetClientId)
        {
            RequestHomeServerRpc(targetClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestHomeServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            if (_homeSnapshots.TryGetValue(targetClientId, out string json))
            {
                ulong requester = rpcParams.Receive.SenderClientId;
                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { requester }
                    }
                };
                SendHomeToClientRpc(targetClientId, json, clientRpcParams);
            }
        }

        // ── RPCs ────────────────────────────────────────────────────────────────

        [ClientRpc]
        private void BroadcastHomeClientRpc(ulong ownerClientId, string homeJson)
        {
            // Don't re-load your own home.
            if (ownerClientId == NetworkManager.Singleton.LocalClientId) return;

            OnHomeSnapshotReceived.Invoke(ownerClientId.ToString(), homeJson);
        }

        [ClientRpc]
        private void SendHomeToClientRpc(ulong ownerClientId, string homeJson, ClientRpcParams clientRpcParams = default)
        {
            OnHomeSnapshotReceived.Invoke(ownerClientId.ToString(), homeJson);
        }

        // ── Lobby Info ──────────────────────────────────────────────────────────

        /// <summary>Returns the client IDs of all players who have uploaded a home.</summary>
        public IReadOnlyCollection<ulong> GetPlayersWithHomes() => _homeSnapshots.Keys;
    }
}
