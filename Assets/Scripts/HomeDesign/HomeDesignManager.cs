using System;
using System.Collections.Generic;
using UnityEngine;
using ImpressMyGuests.CharacterCreation;

namespace ImpressMyGuests.HomeDesign
{
    /// <summary>
    /// Top-level manager for the home-design session. Coordinates the active room,
    /// the player's budget, and the save/load of home layouts.
    /// </summary>
    public class HomeDesignManager : MonoBehaviour
    {
        public static HomeDesignManager Instance { get; private set; }

        [Header("Catalog")]
        [SerializeField] private FurnitureCatalog furnitureCatalog;
        public FurnitureCatalog FurnitureCatalog => furnitureCatalog;

        [Header("Budget")]
        [SerializeField] private int startingBudget = 5000;
        public int RemainingBudget { get; private set; }

        [Header("Rooms")]
        [SerializeField] private List<RoomManager> rooms = new List<RoomManager>();
        public RoomManager ActiveRoom { get; private set; }

        // The owner of this home (set from CharacterCreator after character creation).
        public CharacterData OwnerCharacter { get; private set; }

        // Home identifier used for network synchronisation.
        public string HomeId { get; private set; }

        public event Action<int> OnBudgetChanged;
        public event Action<RoomManager> OnActiveRoomChanged;
        public event Action<FurnitureItem> OnItemPlaced;
        public event Action<FurnitureItem> OnItemRemoved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            RemainingBudget = startingBudget;
            HomeId = Guid.NewGuid().ToString("N")[..8];
        }

        private void Start()
        {
            OwnerCharacter = CharacterCreator.ConfirmedCharacter;

            if (rooms.Count > 0)
                SetActiveRoom(rooms[0]);

            foreach (var room in rooms)
            {
                room.OnItemPlaced += HandleItemPlaced;
                room.OnItemRemoved += HandleItemRemoved;
            }
        }

        private void OnDestroy()
        {
            foreach (var room in rooms)
            {
                if (room == null) continue;
                room.OnItemPlaced -= HandleItemPlaced;
                room.OnItemRemoved -= HandleItemRemoved;
            }
        }

        // ── Room Selection ──────────────────────────────────────────────────────

        public void SetActiveRoom(RoomManager room)
        {
            ActiveRoom = room;
            OnActiveRoomChanged?.Invoke(room);
        }

        public void SetActiveRoom(int index)
        {
            if (index < 0 || index >= rooms.Count) return;
            SetActiveRoom(rooms[index]);
        }

        // ── Placement ───────────────────────────────────────────────────────────

        /// <summary>
        /// Spawns a furniture prefab from the catalog and attempts to place it in the
        /// active room. Returns the placed item or null on failure.
        /// </summary>
        public FurnitureItem PlaceFurniture(string itemId, Vector2Int cell)
        {
            if (ActiveRoom == null)
            {
                Debug.LogWarning("[HomeDesignManager] No active room set.");
                return null;
            }

            var prefab = furnitureCatalog.GetPrefab(itemId);
            if (prefab == null) return null;

            var go = Instantiate(prefab);
            var item = go.GetComponent<FurnitureItem>();
            if (item == null)
            {
                Destroy(go);
                Debug.LogError($"[HomeDesignManager] Prefab for '{itemId}' has no FurnitureItem component.");
                return null;
            }

            if (!ActiveRoom.TryPlaceItem(item, cell))
            {
                Destroy(go);
                return null;
            }

            SpendBudget(item.purchaseCost);
            return item;
        }

        public void RemoveFurniture(FurnitureItem item)
        {
            if (item == null || !item.isPlaced) return;
            ActiveRoom?.RemoveItem(item);
            RefundBudget(item.purchaseCost);
            Destroy(item.gameObject);
        }

        // ── Budget ──────────────────────────────────────────────────────────────

        private void SpendBudget(int amount)
        {
            RemainingBudget = Mathf.Max(0, RemainingBudget - amount);
            OnBudgetChanged?.Invoke(RemainingBudget);
        }

        private void RefundBudget(int amount)
        {
            RemainingBudget += amount;
            OnBudgetChanged?.Invoke(RemainingBudget);
        }

        public bool CanAfford(int cost) => RemainingBudget >= cost;

        // ── Event Relaying ──────────────────────────────────────────────────────

        private void HandleItemPlaced(FurnitureItem item)   => OnItemPlaced?.Invoke(item);
        private void HandleItemRemoved(FurnitureItem item)  => OnItemRemoved?.Invoke(item);

        // ── Save/Load ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a serialisable snapshot of the entire home for network transmission.
        /// </summary>
        public HomeSnapshot GetSnapshot()
        {
            var snapshot = new HomeSnapshot { homeId = HomeId };
            foreach (var room in rooms)
                snapshot.rooms.Add(new RoomSnapshot { roomId = room.roomId, items = room.GetSnapshot() });
            return snapshot;
        }
    }

    // ── Snapshot types ───────────────────────────────────────────────────────────

    [Serializable]
    public class HomeSnapshot
    {
        public string homeId;
        public List<RoomSnapshot> rooms = new List<RoomSnapshot>();
    }

    [Serializable]
    public class RoomSnapshot
    {
        public string roomId;
        public List<PlacedItemSnapshot> items = new List<PlacedItemSnapshot>();
    }
}
