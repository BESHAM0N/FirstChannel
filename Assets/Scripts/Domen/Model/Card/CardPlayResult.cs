using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class CardPlayResult
    {
        private readonly Dictionary<GuestId, int> _emotionChanges;

        public Card Card { get; }
        public GuestId DialogueGuestId { get; }
        public int AddedViewers { get; }

        public CardPlayResult(
            Card card,
            GuestId dialogueGuestId,
            int addedViewers,
            Dictionary<GuestId, int> emotionChanges)
        {
            Card = card;
            DialogueGuestId = dialogueGuestId;
            AddedViewers = addedViewers;
            _emotionChanges = emotionChanges;
        }

        public bool TryGetEmotionChange(GuestId guestId, out int value)
        {
            return _emotionChanges.TryGetValue(guestId, out value);
        }
    }
}
