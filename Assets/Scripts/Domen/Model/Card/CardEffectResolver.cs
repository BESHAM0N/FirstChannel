using System;
using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class CardEffectResolver
    {
        private const int WeepingCriticalViewers = 300;
        private const int BoostMultiplier = 2;

        private readonly GuestRoster _guestRoster;
        private bool _isProvocationBoostArmed;

        public CardEffectResolver(GuestRoster guestRoster)
        {
            _guestRoster = guestRoster;
        }

        public CardPlayResult Resolve(Card card, GuestId targetGuestId)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            bool isBoosted = _isProvocationBoostArmed && IsProvocation(card);
            int multiplier = isBoosted ? BoostMultiplier : 1;
            int viewers = ResolveViewers(card, targetGuestId) * multiplier;
            int emotion = card.AddEmotionCharacter * multiplier;
            var emotionChanges = new Dictionary<GuestId, int>(2);

            if (AffectsBothGuests(card))
            {
                emotionChanges.Add(GuestId.First, emotion);
                emotionChanges.Add(GuestId.Second, emotion);
            }
            else
            {
                emotionChanges.Add(targetGuestId, emotion);
            }

            if (isBoosted)
                _isProvocationBoostArmed = false;

            if (card.CardType == CardType.UnexpectedWitness)
                _isProvocationBoostArmed = true;

            return new CardPlayResult(card, targetGuestId, viewers, emotionChanges);
        }

        public int GetDisplayedEmotion(Card card)
        {
            return IsBoosted(card)
                ? card.AddEmotionCharacter * BoostMultiplier
                : card.AddEmotionCharacter;
        }

        public int GetDisplayedViewers(Card card)
        {
            return IsBoosted(card)
                ? card.AddViewers * BoostMultiplier
                : card.AddViewers;
        }

        private int ResolveViewers(Card card, GuestId targetGuestId)
        {
            if (card.CardType != CardType.Weeping)
                return card.AddViewers;

            return _guestRoster.TryGetGuest(targetGuestId, out Guest guest) &&
                   guest.EmotionBar.CurrentValue <= EmotionBar.SecondFireThreshold
                ? WeepingCriticalViewers
                : card.AddViewers;
        }

        private bool IsBoosted(Card card)
        {
            return card != null && _isProvocationBoostArmed && IsProvocation(card);
        }

        private static bool IsProvocation(Card card)
        {
            return card.CardType == CardType.Claim ||
                   card.CardType == CardType.Compromat ||
                   card.CardType == CardType.SENSATION;
        }

        private static bool AffectsBothGuests(Card card)
        {
            return card.CardType == CardType.AdvertisingBreak ||
                   card.CardType == CardType.SENSATION;
        }
    }
}
