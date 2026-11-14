using UnityEngine;

namespace FirstChannel
{
    [System.Serializable]
    public struct CardVariantConfig
    {
        public CardConfig FirstGuest;
        public CardConfig SecondGuest;
    }

    [CreateAssetMenu(fileName = "DeckCardConfig", menuName = "Configs/Deck Card")]
    public sealed class DeckCardConfig : ScriptableObject
    {
        [Tooltip("21 pair: one card configuration for each guest in every pair.")]
        public CardVariantConfig[] CardVariants;
    }
}
