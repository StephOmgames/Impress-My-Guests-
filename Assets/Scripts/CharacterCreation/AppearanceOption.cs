using System;
using UnityEngine;

namespace ImpressMyGuests.CharacterCreation
{
    /// <summary>
    /// Represents a single appearance option that can be displayed in the
    /// character-creation UI (e.g. a hair-style option with a preview sprite).
    /// </summary>
    [CreateAssetMenu(fileName = "AppearanceOption", menuName = "ImpressMyGuests/Appearance Option")]
    public class AppearanceOption : ScriptableObject
    {
        [Header("Identity")]
        public string optionName;
        [TextArea(2, 4)]
        public string description;
        public Sprite previewSprite;

        [Header("Category")]
        public AppearanceCategory category;

        /// <summary>
        /// Generic int value that maps to one of the enum variants (HairStyle, SkinTone, etc.)
        /// for the relevant category. Cast at the call site.
        /// </summary>
        public int enumValue;

        [Header("Unlock")]
        public bool isUnlockedByDefault = true;
        public int unlockCost = 0;
    }

    public enum AppearanceCategory
    {
        BodyType,
        SkinTone,
        HairStyle,
        HairColor,
        EyeColor,
        OutfitStyle
    }
}
