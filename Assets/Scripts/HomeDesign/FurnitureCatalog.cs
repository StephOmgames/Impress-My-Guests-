using System.Collections.Generic;
using UnityEngine;

namespace ImpressMyGuests.HomeDesign
{
    /// <summary>
    /// ScriptableObject catalog of all furniture items available in the game.
    /// Assign prefabs here; <see cref="HomeDesignManager"/> uses this to instantiate items.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureCatalog", menuName = "ImpressMyGuests/Furniture Catalog")]
    public class FurnitureCatalog : ScriptableObject
    {
        [SerializeField] private List<FurnitureEntry> entries = new List<FurnitureEntry>();

        /// <summary>Returns the prefab whose <see cref="FurnitureItem.itemId"/> matches, or null.</summary>
        public GameObject GetPrefab(string itemId)
        {
            foreach (var entry in entries)
            {
                if (entry.itemId == itemId)
                    return entry.prefab;
            }
            Debug.LogWarning($"[FurnitureCatalog] No entry found for itemId '{itemId}'.");
            return null;
        }

        /// <summary>Returns all entries in the catalog.</summary>
        public IReadOnlyList<FurnitureEntry> GetAllEntries() => entries;

        /// <summary>Returns entries filtered by category.</summary>
        public List<FurnitureEntry> GetByCategory(FurnitureCategory category)
        {
            var result = new List<FurnitureEntry>();
            foreach (var entry in entries)
            {
                if (entry.category == category)
                    result.Add(entry);
            }
            return result;
        }
    }

    [System.Serializable]
    public class FurnitureEntry
    {
        public string itemId;
        public string displayName;
        public FurnitureCategory category;
        public GameObject prefab;
        public Sprite thumbnail;
        public int cost;
        public bool isDefault = true;
    }
}
