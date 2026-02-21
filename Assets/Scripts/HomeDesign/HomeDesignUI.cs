using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ImpressMyGuests.HomeDesign
{
    /// <summary>
    /// Drives the home-design UI. Presents the furniture catalog, handles drag-and-drop
    /// placement, and updates the budget display.
    /// </summary>
    public class HomeDesignUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HomeDesignManager homeDesignManager;

        [Header("Catalog Panel")]
        [SerializeField] private Transform catalogContainer;
        [SerializeField] private GameObject catalogItemButtonPrefab;

        [Header("Room Tabs")]
        [SerializeField] private Transform roomTabContainer;
        [SerializeField] private GameObject roomTabButtonPrefab;

        [Header("Category Filter")]
        [SerializeField] private TMP_Dropdown categoryFilterDropdown;

        [Header("Budget Display")]
        [SerializeField] private TMP_Text budgetText;

        [Header("Selected Item Info")]
        [SerializeField] private TMP_Text selectedItemNameText;
        [SerializeField] private TMP_Text selectedItemDescText;
        [SerializeField] private Image selectedItemThumbnail;

        [Header("Buttons")]
        [SerializeField] private Button removeItemButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;

        private string _selectedItemId;
        private FurnitureItem _selectedPlacedItem;

        private void Awake()
        {
            PopulateCategoryDropdown();

            categoryFilterDropdown.onValueChanged.AddListener(_ => RefreshCatalog());
            removeItemButton.onClick.AddListener(OnRemoveClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            homeDesignManager.OnBudgetChanged += RefreshBudget;
            homeDesignManager.OnActiveRoomChanged += RefreshRoomTabs;
            RefreshBudget(homeDesignManager.RemainingBudget);
            RefreshCatalog();
        }

        private void OnDisable()
        {
            homeDesignManager.OnBudgetChanged -= RefreshBudget;
            homeDesignManager.OnActiveRoomChanged -= RefreshRoomTabs;
        }

        // ── Catalog ─────────────────────────────────────────────────────────────

        private void RefreshCatalog()
        {
            foreach (Transform child in catalogContainer)
                Destroy(child.gameObject);

            if (homeDesignManager.FurnitureCatalog == null) return;

            // Determine which category to show (index 0 = "All").
            bool showAll = categoryFilterDropdown.value == 0;
            FurnitureCategory selectedCategory = (FurnitureCategory)(categoryFilterDropdown.value - 1);

            foreach (var entry in homeDesignManager.FurnitureCatalog.GetAllEntries())
            {
                if (!showAll && entry.category != selectedCategory) continue;

                var btnGo = Instantiate(catalogItemButtonPrefab, catalogContainer);
                var btn = btnGo.GetComponent<Button>();
                var label = btnGo.GetComponentInChildren<TMP_Text>();
                var image = btnGo.GetComponentInChildren<Image>();

                if (label != null) label.text = entry.displayName;
                if (image != null && entry.thumbnail != null) image.sprite = entry.thumbnail;

                string capturedId = entry.itemId;
                string capturedName = entry.displayName;
                Sprite capturedThumb = entry.thumbnail;
                btn?.onClick.AddListener(() =>
                    SelectCatalogItem(capturedId, capturedName, string.Empty, capturedThumb));
            }
        }

        // ── Budget ──────────────────────────────────────────────────────────────

        private void RefreshBudget(int remaining)
        {
            if (budgetText != null)
                budgetText.text = $"Budget: ${remaining:N0}";
        }

        // ── Room Tabs ───────────────────────────────────────────────────────────

        private void RefreshRoomTabs(RoomManager activeRoom)
        {
            if (activeRoom == null) return;
            // Room tab highlighting could be updated here.
        }

        // ── Item Selection ──────────────────────────────────────────────────────

        public void SelectCatalogItem(string itemId, string displayName, string desc, Sprite thumbnail)
        {
            _selectedItemId = itemId;
            _selectedPlacedItem = null;

            if (selectedItemNameText != null) selectedItemNameText.text = displayName;
            if (selectedItemDescText != null) selectedItemDescText.text = desc;
            if (selectedItemThumbnail != null) selectedItemThumbnail.sprite = thumbnail;

            removeItemButton.interactable = false;
        }

        public void SelectPlacedItem(FurnitureItem item)
        {
            _selectedPlacedItem = item;
            _selectedItemId = null;

            if (selectedItemNameText != null) selectedItemNameText.text = item.displayName;
            if (selectedItemDescText != null) selectedItemDescText.text = item.description;
            if (selectedItemThumbnail != null) selectedItemThumbnail.sprite = item.thumbnailSprite;

            removeItemButton.interactable = true;
        }

        // ── Button Callbacks ────────────────────────────────────────────────────

        private void OnRemoveClicked()
        {
            if (_selectedPlacedItem != null)
            {
                homeDesignManager.RemoveFurniture(_selectedPlacedItem);
                _selectedPlacedItem = null;
                removeItemButton.interactable = false;
            }
        }

        private void OnSaveClicked()
        {
            var snapshot = homeDesignManager.GetSnapshot();
            string json = UnityEngine.JsonUtility.ToJson(snapshot, true);
            Debug.Log($"[HomeDesignUI] Home saved:\n{json}");
            // In a full implementation, send to the network or write to disk.
        }

        private void OnBackClicked()
        {
            ImpressMyGuests.Core.GameManager.Instance.GoToMainMenu();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void PopulateCategoryDropdown()
        {
            categoryFilterDropdown.ClearOptions();
            var options = new List<string> { "All" };
            foreach (var name in System.Enum.GetNames(typeof(FurnitureCategory)))
                options.Add(name);
            categoryFilterDropdown.AddOptions(options);
        }
    }
}
