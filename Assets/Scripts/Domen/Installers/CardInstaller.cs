using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class CardInstaller : Installer
    {
        [SerializeField] private DeckCardConfig _deckConfig;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private HandCardView _handView;
        [SerializeField] private DeckCardView _deckView;
        [SerializeField] private bool _shuffleDeck = true;

        public override void InstallBindings()
        {
            Container.Bind<HandCardView>().FromInstance(_handView).AsSingle();
            Container.Bind<DeckCardView>().FromInstance(_deckView).AsSingle();

            Container.Bind<DeckCard>().AsSingle();
            Container.Bind<HandCards>().AsSingle();
            Container.Bind<CardFactory>().AsSingle();
            Container.Bind<CardEffectResolver>().AsSingle();

            Container
                .BindFactory<CardView, CardView.Factory>()
                .FromComponentInNewPrefab(_cardPrefab)
                .UnderTransform(_handView.transform);

            Container
                .BindInterfacesAndSelfTo<CardHandPresenter>()
                .AsSingle()
                .WithArguments(_deckConfig, _shuffleDeck)
                .NonLazy();
        }
    }
}
