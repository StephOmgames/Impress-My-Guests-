using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ImpressMyGuests.CharacterCreation
{
    /// <summary>
    /// Drives the character-creation UI. Subscribes to <see cref="CharacterCreator"/> events
    /// and updates the preview panel in real time.
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterCreator characterCreator;

        [Header("Name Input")]
        [SerializeField] private TMP_InputField nameInputField;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown bodyTypeDropdown;
        [SerializeField] private TMP_Dropdown skinToneDropdown;
        [SerializeField] private TMP_Dropdown hairStyleDropdown;
        [SerializeField] private TMP_Dropdown hairColorDropdown;
        [SerializeField] private TMP_Dropdown eyeColorDropdown;
        [SerializeField] private TMP_Dropdown outfitStyleDropdown;
        [SerializeField] private TMP_Dropdown personalityTraitDropdown;

        [Header("Preview")]
        [SerializeField] private TMP_Text previewLabel;
        [SerializeField] private Image characterPreviewImage;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button resetButton;

        [Header("Error Display")]
        [SerializeField] private TMP_Text errorText;

        private void Awake()
        {
            PopulateDropdowns();

            // Wire up UI events
            nameInputField.onValueChanged.AddListener(characterCreator.SetName);

            bodyTypeDropdown.onValueChanged.AddListener(v => characterCreator.SetBodyType((BodyType)v));
            skinToneDropdown.onValueChanged.AddListener(v => characterCreator.SetSkinTone((SkinTone)v));
            hairStyleDropdown.onValueChanged.AddListener(v => characterCreator.SetHairStyle((HairStyle)v));
            hairColorDropdown.onValueChanged.AddListener(v => characterCreator.SetHairColor((HairColor)v));
            eyeColorDropdown.onValueChanged.AddListener(v => characterCreator.SetEyeColor((EyeColor)v));
            outfitStyleDropdown.onValueChanged.AddListener(v => characterCreator.SetOutfitStyle((OutfitStyle)v));
            personalityTraitDropdown.onValueChanged.AddListener(v => characterCreator.SetPersonalityTrait((PersonalityTrait)v));

            confirmButton.onClick.AddListener(OnConfirmClicked);
            resetButton.onClick.AddListener(OnResetClicked);
        }

        private void OnEnable()
        {
            characterCreator.OnCharacterChanged += RefreshPreview;
            RefreshPreview(characterCreator.Draft);
        }

        private void OnDisable()
        {
            characterCreator.OnCharacterChanged -= RefreshPreview;
        }

        // ── Initialisation ──────────────────────────────────────────────────────

        private void PopulateDropdowns()
        {
            PopulateFromEnum<BodyType>(bodyTypeDropdown);
            PopulateFromEnum<SkinTone>(skinToneDropdown);
            PopulateFromEnum<HairStyle>(hairStyleDropdown);
            PopulateFromEnum<HairColor>(hairColorDropdown);
            PopulateFromEnum<EyeColor>(eyeColorDropdown);
            PopulateFromEnum<OutfitStyle>(outfitStyleDropdown);
            PopulateFromEnum<PersonalityTrait>(personalityTraitDropdown);
        }

        private static void PopulateFromEnum<T>(TMP_Dropdown dropdown) where T : System.Enum
        {
            dropdown.ClearOptions();
            var options = new List<string>(System.Enum.GetNames(typeof(T)));
            dropdown.AddOptions(options);
        }

        // ── Callbacks ───────────────────────────────────────────────────────────

        private void RefreshPreview(CharacterData data)
        {
            if (previewLabel != null)
                previewLabel.text = data.ToString();

            if (errorText != null)
                errorText.text = string.Empty;
        }

        private void OnConfirmClicked()
        {
            if (!characterCreator.IsValid(out string error))
            {
                if (errorText != null)
                    errorText.text = error;
                return;
            }
            characterCreator.TryConfirmCharacter();
        }

        private void OnResetClicked()
        {
            characterCreator.ResetDraft();

            // Reset UI controls to defaults
            nameInputField.SetTextWithoutNotify(string.Empty);
            bodyTypeDropdown.SetValueWithoutNotify(0);
            skinToneDropdown.SetValueWithoutNotify(0);
            hairStyleDropdown.SetValueWithoutNotify(0);
            hairColorDropdown.SetValueWithoutNotify(0);
            eyeColorDropdown.SetValueWithoutNotify(0);
            outfitStyleDropdown.SetValueWithoutNotify(0);
            personalityTraitDropdown.SetValueWithoutNotify(0);

            if (errorText != null)
                errorText.text = string.Empty;
        }
    }
}
