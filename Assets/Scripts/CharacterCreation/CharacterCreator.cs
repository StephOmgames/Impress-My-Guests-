using System;
using System.Collections.Generic;
using UnityEngine;
using ImpressMyGuests.Core;

namespace ImpressMyGuests.CharacterCreation
{
    /// <summary>
    /// Manages the character-creation flow. Holds the in-progress <see cref="CharacterData"/>,
    /// validates completeness, and fires events that the UI layer reacts to.
    /// </summary>
    public class CharacterCreator : MonoBehaviour
    {
        public static CharacterCreator Instance { get; private set; }

        // Fired whenever any field on the draft character changes.
        public event Action<CharacterData> OnCharacterChanged;

        // Fired when the player confirms their character.
        public event Action<CharacterData> OnCharacterConfirmed;

        [Header("Available Options")]
        [SerializeField] private List<AppearanceOption> availableOptions = new List<AppearanceOption>();

        // The character being built. Public for read access by the UI.
        public CharacterData Draft { get; private set; } = new CharacterData();

        // Stores the confirmed character so other systems can retrieve it.
        public static CharacterData ConfirmedCharacter { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // ── Mutators ────────────────────────────────────────────────────────────

        public void SetName(string newName)
        {
            Draft.characterName = string.IsNullOrWhiteSpace(newName) ? "Player" : newName.Trim();
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetBodyType(BodyType bodyType)
        {
            Draft.bodyType = bodyType;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetSkinTone(SkinTone skinTone)
        {
            Draft.skinTone = skinTone;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetHairStyle(HairStyle hairStyle)
        {
            Draft.hairStyle = hairStyle;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetHairColor(HairColor hairColor)
        {
            Draft.hairColor = hairColor;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetEyeColor(EyeColor eyeColor)
        {
            Draft.eyeColor = eyeColor;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetOutfitStyle(OutfitStyle outfitStyle)
        {
            Draft.outfitStyle = outfitStyle;
            OnCharacterChanged?.Invoke(Draft);
        }

        public void SetPersonalityTrait(PersonalityTrait trait)
        {
            Draft.primaryTrait = trait;
            OnCharacterChanged?.Invoke(Draft);
        }

        // ── Confirmation ────────────────────────────────────────────────────────

        /// <summary>
        /// Validates the draft and, if valid, confirms the character and proceeds
        /// to the home-design scene.
        /// </summary>
        public bool TryConfirmCharacter()
        {
            if (!IsValid(out string error))
            {
                Debug.LogWarning($"[CharacterCreator] Cannot confirm: {error}");
                return false;
            }

            ConfirmedCharacter = Draft.Clone();
            OnCharacterConfirmed?.Invoke(ConfirmedCharacter);
            Debug.Log($"[CharacterCreator] Character confirmed: {ConfirmedCharacter}");

            GameManager.Instance.StartHomeDesign();
            return true;
        }

        /// <summary>Resets the draft to default values.</summary>
        public void ResetDraft()
        {
            Draft = new CharacterData();
            OnCharacterChanged?.Invoke(Draft);
        }

        // ── Validation ──────────────────────────────────────────────────────────

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(Draft.characterName))
            {
                error = "Character name cannot be empty.";
                return false;
            }
            if (Draft.characterName.Length < 2)
            {
                error = "Character name must be at least 2 characters.";
                return false;
            }
            error = null;
            return true;
        }

        // ── Queries ─────────────────────────────────────────────────────────────

        /// <summary>Returns all options for a given appearance category.</summary>
        public IReadOnlyList<AppearanceOption> GetOptionsForCategory(AppearanceCategory category)
        {
            var result = new List<AppearanceOption>();
            foreach (var opt in availableOptions)
            {
                if (opt.category == category)
                    result.Add(opt);
            }
            return result;
        }
    }
}
