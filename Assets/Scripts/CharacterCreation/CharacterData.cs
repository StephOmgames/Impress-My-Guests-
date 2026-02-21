using System;
using UnityEngine;

namespace ImpressMyGuests.CharacterCreation
{
    /// <summary>
    /// Serialisable data container that holds all choices made during character creation.
    /// This object is passed to <see cref="GameManager"/> and networked to other players.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        [Header("Identity")]
        public string characterName = "Player";

        [Header("Appearance")]
        public BodyType bodyType = BodyType.Average;
        public SkinTone skinTone = SkinTone.Medium;
        public HairStyle hairStyle = HairStyle.ShortWavy;
        public HairColor hairColor = HairColor.Brown;
        public EyeColor eyeColor = EyeColor.Brown;
        public OutfitStyle outfitStyle = OutfitStyle.Casual;

        [Header("Personality")]
        public PersonalityTrait primaryTrait = PersonalityTrait.Friendly;

        /// <summary>Creates a deep copy of this data object.</summary>
        public CharacterData Clone()
        {
            return new CharacterData
            {
                characterName = characterName,
                bodyType = bodyType,
                skinTone = skinTone,
                hairStyle = hairStyle,
                hairColor = hairColor,
                eyeColor = eyeColor,
                outfitStyle = outfitStyle,
                primaryTrait = primaryTrait
            };
        }

        public override string ToString()
        {
            return $"{characterName} | {bodyType} | {skinTone} skin | {hairStyle} {hairColor} hair | " +
                   $"{eyeColor} eyes | {outfitStyle} outfit | {primaryTrait}";
        }
    }

    public enum BodyType   { Slim, Average, Athletic, Curvy }
    public enum SkinTone   { Fair, Light, Medium, Tan, Dark, Deep }
    public enum HairStyle  { Bald, ShortStraight, ShortWavy, MediumStraight, MediumCurly, LongStraight, LongCurly, BraidsOrLocs }
    public enum HairColor  { Black, Brown, Blonde, Auburn, Red, Grey, White, Blue, Pink, Purple, Green }
    public enum EyeColor   { Brown, Blue, Green, Hazel, Grey, Amber }
    public enum OutfitStyle { Casual, Formal, Sporty, Vintage, Fantasy, Modern }
    public enum PersonalityTrait { Friendly, Artistic, Adventurous, Cozy, Sophisticated, Playful }
}
