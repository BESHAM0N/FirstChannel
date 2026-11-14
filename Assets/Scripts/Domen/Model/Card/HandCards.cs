using System;
using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class HandCards
    {
        public const int MaxCards = 3;

        public event Action Changed;

        private readonly List<Card> _cards = new List<Card>(MaxCards);

        public int Count => _cards.Count;
        public bool IsFull => _cards.Count >= MaxCards;
        public IReadOnlyList<Card> Cards => _cards;

        public bool TryAdd(Card card)
        {
            if (card == null || _cards.Count >= MaxCards)
                return false;

            _cards.Add(card);
            Changed?.Invoke();
            return true;
        }

        public bool TryRemove(Card card)
        {
            if (card == null || !_cards.Remove(card))
                return false;

            Changed?.Invoke();
            return true;
        }
    }
}
