using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImpressMyGuests.HomeDesign
{
    /// <summary>
    /// Manages a single room's grid, tracks placed furniture, and validates placements.
    /// Each room in a home has its own RoomManager component.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [Header("Room Identity")]
        public string roomId;
        public string roomName = "Living Room";
        public RoomType roomType = RoomType.LivingRoom;

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float cellSize = 1f;

        // True if a cell is occupied.
        private bool[,] _grid;
        private readonly List<FurnitureItem> _placedItems = new List<FurnitureItem>();

        public event Action<FurnitureItem> OnItemPlaced;
        public event Action<FurnitureItem> OnItemRemoved;

        private void Awake()
        {
            _grid = new bool[gridWidth, gridHeight];
        }

        // ── Placement ───────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to place <paramref name="item"/> at <paramref name="cell"/>.
        /// Returns false if the space is occupied or out of bounds.
        /// </summary>
        public bool TryPlaceItem(FurnitureItem item, Vector2Int cell)
        {
            if (!CanPlace(item, cell)) return false;

            OccupyCells(item, cell, true);
            item.PlaceAt(cell, cellSize);
            item.transform.SetParent(transform, true);
            if (!_placedItems.Contains(item))
            _placedItems.Add(item);
            OnItemPlaced?.Invoke(item);
            return true;
        }

        /// <summary>Lifts an already-placed item back off the grid.</summary>
        public void RemoveItem(FurnitureItem item)
        {
            if (!item.isPlaced) return;

            OccupyCells(item, item.gridPosition, false);
            item.Lift();
            _placedItems.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        /// <summary>Moves a placed item to a new cell.</summary>
        public bool TryMoveItem(FurnitureItem item, Vector2Int newCell)
        {
            if (!item.isPlaced) return false;

            Vector2Int oldCell = item.gridPosition;
            OccupyCells(item, oldCell, false);

            if (!CanPlace(item, newCell))
            {
                OccupyCells(item, oldCell, true);
                return false;
            }

            OccupyCells(item, newCell, true);
            item.PlaceAt(newCell, cellSize);
            return true;
        }

        // ── Queries ─────────────────────────────────────────────────────────────

        public bool CanPlace(FurnitureItem item, Vector2Int cell)
        {
            int w = Mathf.RoundToInt(item.gridSize.x);
            int h = Mathf.RoundToInt(item.gridSize.y);

            for (int x = cell.x; x < cell.x + w; x++)
            {
                for (int y = cell.y; y < cell.y + h; y++)
                {
                    if (!IsInBounds(x, y) || _grid[x, y])
                        return false;
                }
            }
            return true;
        }

        public bool IsInBounds(int x, int y) =>
            x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;

        public IReadOnlyList<FurnitureItem> GetPlacedItems() => _placedItems;

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void OccupyCells(FurnitureItem item, Vector2Int cell, bool occupied)
        {
            int w = Mathf.RoundToInt(item.gridSize.x);
            int h = Mathf.RoundToInt(item.gridSize.y);

            for (int x = cell.x; x < cell.x + w; x++)
            {
                for (int y = cell.y; y < cell.y + h; y++)
                {
                    if (IsInBounds(x, y))
                        _grid[x, y] = occupied;
                }
            }
        }

        // ── Serialisation (for network sync) ────────────────────────────────────

        /// <summary>
        /// Returns a lightweight snapshot of all placed items for network synchronisation.
        /// </summary>
        public List<PlacedItemSnapshot> GetSnapshot()
        {
            var snapshot = new List<PlacedItemSnapshot>();
            foreach (var item in _placedItems)
            {
                snapshot.Add(new PlacedItemSnapshot
                {
                    itemId = item.itemId,
                    cell = item.gridPosition,
                    rotation = item.transform.localRotation.eulerAngles.y
                });
            }
            return snapshot;
        }
    }

    public enum RoomType
    {
        LivingRoom,
        Kitchen,
        Bedroom,
        Bathroom,
        DiningRoom,
        Garden,
        Hallway
    }

    [Serializable]
    public class PlacedItemSnapshot
    {
        public string itemId;
        public Vector2Int cell;
        public float rotation;
    }
}
