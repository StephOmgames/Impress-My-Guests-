using Unity.Netcode;
using UnityEngine;
using ImpressMyGuests.CharacterCreation;

namespace ImpressMyGuests.Multiplayer
{
    /// <summary>
    /// Network-aware component attached to each player's avatar prefab.
    /// Synchronises character data and the player's position/rotation over the network.
    /// </summary>
    public class PlayerNetworkController : NetworkBehaviour
    {
        // ── Networked character display name ────────────────────────────────────
        private NetworkVariable<Unity.Collections.FixedString64Bytes> _networkName =
            new NetworkVariable<Unity.Collections.FixedString64Bytes>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        [Header("Visual")]
        [SerializeField] private TMPro.TMP_Text nameLabel;

        // Locally-owned character data (not fully synchronised — only display name
        // is networked; full CharacterData is sent once via RPC).
        private CharacterData _characterData;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _characterData = CharacterCreator.ConfirmedCharacter;
                string playerName = _characterData?.characterName ?? $"Player {OwnerClientId}";
                _networkName.Value = playerName;
                SendCharacterDataServerRpc(playerName);
            }

            _networkName.OnValueChanged += OnNameChanged;
            RefreshNameLabel(_networkName.Value.ToString());
        }

        public override void OnNetworkDespawn()
        {
            _networkName.OnValueChanged -= OnNameChanged;
        }

        // ── RPCs ────────────────────────────────────────────────────────────────

        [ServerRpc]
        private void SendCharacterDataServerRpc(string characterName, ServerRpcParams rpcParams = default)
        {
            // Broadcast to all clients so they can show the correct name tag.
            UpdateNameClientRpc(characterName);
        }

        [ClientRpc]
        private void UpdateNameClientRpc(string characterName)
        {
            RefreshNameLabel(characterName);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void OnNameChanged(
            Unity.Collections.FixedString64Bytes previous,
            Unity.Collections.FixedString64Bytes current)
        {
            RefreshNameLabel(current.ToString());
        }

        private void RefreshNameLabel(string text)
        {
            if (nameLabel != null)
                nameLabel.text = text;
        }
    }
}
