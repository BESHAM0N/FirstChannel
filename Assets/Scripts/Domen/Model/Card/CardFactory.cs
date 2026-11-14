using System;
using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class CardFactory
    {
        private readonly CardView.Factory _viewFactory;

        public CardFactory(CardView.Factory viewFactory)
        {
            _viewFactory = viewFactory;
        }

        public List<Card> BuildModels(DeckCardConfig source)
        {
            if (source?.CardVariants == null)
                throw new InvalidOperationException("Deck card variants are not configured.");

            if (source.CardVariants.Length != DeckCard.RequiredCards)
            {
                throw new InvalidOperationException(
                    $"Deck must contain exactly {DeckCard.RequiredCards} card variant pairs.");
            }

            var result = new List<Card>(DeckCard.RequiredCards);

            for (int i = 0; i < source.CardVariants.Length; i++)
            {
                CardVariantConfig variant = source.CardVariants[i];

                if (variant.FirstGuest == null || variant.SecondGuest == null)
                    throw new InvalidOperationException($"Card variant pair {i + 1} is incomplete.");

                CardConfig selected = UnityEngine.Random.Range(0, 2) == 0
                    ? variant.FirstGuest
                    : variant.SecondGuest;

                var model = new Card();
                model.Initialize(selected);
                result.Add(model);
            }

            return result;
        }

        public CardView CreateView(Card model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var view = _viewFactory.Create();
            view.SetCard(model.ImageCard, model.NameCard,  model.DescriptionCard, model.AddEmotionCharacter.ToString(), model.AddViewers.ToString());
            return view;
        }

        public List<CardView> CreateViews(IReadOnlyList<Card> models)
        {
            var list = new List<CardView>(models?.Count ?? 0);
            
            if (models == null) 
                return list;
            
            for (int i = 0; i < models.Count; i++)
                list.Add(CreateView(models[i]));
            
            return list;
        }
    }
}
