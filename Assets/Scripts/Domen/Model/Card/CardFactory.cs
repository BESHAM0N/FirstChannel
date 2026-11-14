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
            if (source?.Cards == null)
                throw new InvalidOperationException("Deck cards are not configured.");

            if (source.Cards.Length != DeckCard.RequiredCards)
            {
                throw new InvalidOperationException(
                    $"Deck must contain exactly {DeckCard.RequiredCards} cards.");
            }

            var result = new List<Card>(DeckCard.RequiredCards);

            for (int i = 0; i < source.Cards.Length; i++)
            {
                CardConfig config = source.Cards[i];

                if (config == null)
                    throw new InvalidOperationException($"Card {i + 1} is not configured.");

                var model = new Card();
                model.Initialize(config);
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
