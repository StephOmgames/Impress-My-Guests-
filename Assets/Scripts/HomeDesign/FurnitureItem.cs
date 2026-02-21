using UnityEngine;

namespace ImpressMyGuests.HomeDesign
{
    /// <summary>
    /// Describes a piece of furniture or décor that can be placed in a room.
    /// Attach this to a prefab's root GameObject.
    /// </summary>
    public class FurnitureItem : MonoBehaviour
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public FurnitureCategory category;

        [Header("Placement")]
        public bool isWallMounted = false;
        public Vector2 gridSize = Vector2.one;          // width × depth in grid cells

        [Header("Cost & Unlock")]
        public int purchaseCost = 0;
        public bool isUnlockedByDefault = true;

        [Header("Preview")]
        public Sprite thumbnailSprite;

        // Runtime state (set by RoomManager during placement)
        [HideInInspector] public Vector2Int gridPosition;
        [HideInInspector] public bool isPlaced = false;

        /// <summary>Snaps this item's world position to the provided grid position.</summary>
        public void PlaceAt(Vector2Int cell, float cellSize)
        {
            gridPosition = cell;
            isPlaced = true;
            transform.localPosition = new Vector3(
                cell.x * cellSize + cellSize * gridSize.x * 0.5f,
                0f,
                cell.y * cellSize + cellSize * gridSize.y * 0.5f
            );
        }

        /// <summary>Removes this item from its current grid position.</summary>
        public void Lift()
        {
            isPlaced = false;
        }
    }

    public enum FurnitureCategory
    {
        Seating,
        Tables,
        Storage,
        Lighting,
        Decor,
        Rugs,
        WallArt,
        Plants,
        Electronics,
        Beds
    }
}
