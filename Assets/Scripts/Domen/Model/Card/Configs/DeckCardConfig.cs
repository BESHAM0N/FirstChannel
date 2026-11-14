using UnityEngine;

namespace FirstChannel
{
    [CreateAssetMenu(fileName = "DeckCardConfig", menuName = "Configs/Deck Card")]
    public sealed class DeckCardConfig : ScriptableObject
    {
        [Tooltip("Ровно 21 карта. Каждую карту можно применить к любому гостю.")]
        public CardConfig[] Cards;
    }
}
