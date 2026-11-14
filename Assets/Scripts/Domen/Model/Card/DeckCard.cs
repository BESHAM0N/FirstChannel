using System;
using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class DeckCard
    {
        public const int RequiredCards = 21;

        public event Action Emptied;

        private readonly List<Card> _cards = new List<Card>();
        private bool _emptyEventRaised;

        public int CardsLeft => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public void SetDeckCards(IEnumerable<Card> cards, bool shuffle)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            var preparedCards = new List<Card>(cards);

            if (preparedCards.Count != RequiredCards)
            {
                throw new InvalidOperationException(
                    $"Deck must contain exactly {RequiredCards} cards, but contains {preparedCards.Count}.");
            }

            _cards.Clear();
            _cards.AddRange(preparedCards);
            _emptyEventRaised = false;

            if (shuffle)
                Shuffle();
        }

        public bool TryDraw(out Card card)
        {
            if (_cards.Count == 0)
            {
                card = null;
                return false;
            }

            int lastIndex = _cards.Count - 1;
            card = _cards[lastIndex];
            _cards.RemoveAt(lastIndex);

            if (_cards.Count == 0 && !_emptyEventRaised)
            {
                _emptyEventRaised = true;
                Emptied?.Invoke();
            }

            return true;
        }

        private void Shuffle()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (_cards[i], _cards[swapIndex]) = (_cards[swapIndex], _cards[i]);
            }
        }
    }
}
