using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class GuestInstaller : Installer
    {
        [Header("С цензурой")]
        [SerializeField] private ReplicaListConfig _firstGuestCensoredReplicas;
        [SerializeField] private ReplicaListConfig _secondGuestCensoredReplicas;

        [Header("Без цензуры")]
        [SerializeField] private ReplicaListConfig _firstGuestUncensoredReplicas;
        [SerializeField] private ReplicaListConfig _secondGuestUncensoredReplicas;

        public override void InstallBindings()
        {
            ReplicaListConfig[] replicas = GameModeSelection.Current == GameMode.WithCensorship
                ? new[] { _firstGuestCensoredReplicas, _secondGuestCensoredReplicas }
                : new[] { _firstGuestUncensoredReplicas, _secondGuestUncensoredReplicas };

            Container.Bind<GuestView>().FromComponentsInHierarchy().AsCached();
            Container.Bind<GuestRoster>().FromInstance(new GuestRoster(replicas)).AsSingle();

            Container
                .BindInterfacesAndSelfTo<GuestDialoguePresenter>()
                .AsSingle()
                .WithArguments(replicas)
                .NonLazy();
        }
    }
}
