using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class CardHandPresenter : IInitializable, IDisposable
    {
        private static readonly SoundType[] CardHoverSounds =
        {
            SoundType.CardHover_1,
            SoundType.CardHover_2,
            SoundType.CardHover_3,
            SoundType.CardHover_4
        };

        private static readonly SoundType[] CardClickSounds =
        {
            SoundType.CardClick_1,
            SoundType.CardClick_2,
            SoundType.CardClick_3,
            SoundType.CardClick_4
        };

        private static readonly SoundType[] TakeCardSounds =
        {
            SoundType.TakeCard_1,
            SoundType.TakeCard_2,
            SoundType.TakeCard_3,
            SoundType.TakeCard_4
        };

        public event Action<CardPlayResult> CardApplied;

        private readonly DeckCardConfig _deckConfig;
        private readonly bool _shuffleDeck;
        private readonly DeckCard _deck;
        private readonly HandCards _hand;
        private readonly CardFactory _cardFactory;
        private readonly CardEffectResolver _effectResolver;
        private readonly ISoundService _soundService;
        private readonly HandCardView _handView;
        private readonly DeckCardView _deckView;
        private readonly Dictionary<CardView, Card> _cardsByView = new Dictionary<CardView, Card>();
        private readonly Dictionary<Card, CardView> _viewsByCard = new Dictionary<Card, CardView>();

        public bool IsInteractionEnabled { get; private set; } = true;

        public CardHandPresenter(
            DeckCardConfig deckConfig,
            bool shuffleDeck,
            DeckCard deck,
            HandCards hand,
            CardFactory cardFactory,
            CardEffectResolver effectResolver,
            ISoundService soundService,
            HandCardView handView,
            DeckCardView deckView)
        {
            _deckConfig = deckConfig;
            _shuffleDeck = shuffleDeck;
            _deck = deck;
            _hand = hand;
            _cardFactory = cardFactory;
            _effectResolver = effectResolver;
            _soundService = soundService;
            _handView = handView;
            _deckView = deckView;
        }

        public void Initialize()
        {
            List<Card> cards = _cardFactory.BuildModels(_deckConfig);
            _deck.SetDeckCards(cards, _shuffleDeck);

            if (cards.Count == 0)
            {
                Debug.LogWarning("DeckCardConfig does not contain cards. The hand cannot be dealt.");
                return;
            }

            DealRoundHand();
        }

        public void Dispose()
        {
            foreach (KeyValuePair<CardView, Card> pair in _cardsByView)
            {
                if (pair.Key == null)
                    continue;

                UnsubscribeFromView(pair.Key);
                UnityEngine.Object.Destroy(pair.Key.gameObject);
            }

            _cardsByView.Clear();
            _viewsByCard.Clear();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            IsInteractionEnabled = enabled;

            foreach (CardView view in _cardsByView.Keys)
                view.SetInteractionEnabled(enabled);
        }

        private void DealRoundHand()
        {
            for (int i = _hand.Count; i < HandCards.MaxCards; i++)
            {
                if (!TryDrawCard())
                    break;
            }

            _handView.LayoutCards(GetOrderedViews(), true);
        }

        private void OnDropRequested(CardView view, GuestView target)
        {
            if (!_cardsByView.TryGetValue(view, out Card card))
                return;

            if (!IsInteractionEnabled)
            {
                view.ReturnToHand();
                return;
            }

            if (target == null)
            {
                view.ReturnToHand();
                return;
            }

            UnsubscribeFromView(view);
            _hand.TryRemove(card);
            _cardsByView.Remove(view);
            _viewsByCard.Remove(card);

            CardPlayResult result = _effectResolver.Resolve(card, target.Id);
            RefreshEffectValues();

            target.PlayCardAppliedFeedback();
            view.PlayApplied(target, () => UnityEngine.Object.Destroy(view.gameObject));

            CardApplied?.Invoke(result);
            TryDrawCard();
            _handView.LayoutCards(GetOrderedViews(), false);
        }

        private bool TryDrawCard()
        {
            if (_hand.IsFull || !_deck.TryDraw(out Card card) || !_hand.TryAdd(card))
                return false;

            CardView view = _cardFactory.CreateView(card);
            view.SetDealOrigin(_deckView.CardOrigin);
            view.SetInteractionEnabled(IsInteractionEnabled);
            SubscribeToView(view);

            _cardsByView.Add(view, card);
            _viewsByCard.Add(card, view);
            _deckView.PlayDrawFeedback();
            return true;
        }

        private void SubscribeToView(CardView view)
        {
            view.DropRequested += OnDropRequested;
            view.Hovered += OnCardHovered;
            view.Clicked += OnCardClicked;
            view.Released += OnCardClicked;
            view.DealStarted += OnCardDealStarted;
        }

        private void UnsubscribeFromView(CardView view)
        {
            view.DropRequested -= OnDropRequested;
            view.Hovered -= OnCardHovered;
            view.Clicked -= OnCardClicked;
            view.Released -= OnCardClicked;
            view.DealStarted -= OnCardDealStarted;
        }

        private void OnCardHovered(CardView view)
        {
            PlayRandom(CardHoverSounds);
        }

        private void OnCardClicked(CardView view)
        {
            PlayRandom(CardClickSounds);
        }

        private void OnCardDealStarted(CardView view)
        {
            PlayRandom(TakeCardSounds);
        }

        private void PlayRandom(IReadOnlyList<SoundType> sounds)
        {
            _soundService.Play(sounds[UnityEngine.Random.Range(0, sounds.Count)]);
        }

        private List<CardView> GetOrderedViews()
        {
            var result = new List<CardView>(_hand.Count);

            for (int i = 0; i < _hand.Cards.Count; i++)
            {
                if (_viewsByCard.TryGetValue(_hand.Cards[i], out CardView view))
                    result.Add(view);
            }

            return result;
        }

        private void RefreshEffectValues()
        {
            foreach (KeyValuePair<Card, CardView> pair in _viewsByCard)
            {
                pair.Value.SetEffectValues(
                    _effectResolver.GetDisplayedEmotion(pair.Key),
                    _effectResolver.GetDisplayedViewers(pair.Key));
            }
        }
    }
}
