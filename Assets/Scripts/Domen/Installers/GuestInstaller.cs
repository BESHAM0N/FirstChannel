using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class GuestInstaller : Installer
    {
        [SerializeField] private ReplicaListConfig _firstGuestReplicas;
        [SerializeField] private ReplicaListConfig _secondGuestReplicas;

        public override void InstallBindings()
        {
            Container.Bind<GuestView>().FromComponentsInHierarchy().AsCached();

            Container
                .BindInterfacesAndSelfTo<GuestDialoguePresenter>()
                .AsSingle()
                .WithArguments(new[] { _firstGuestReplicas, _secondGuestReplicas })
                .NonLazy();
        }
    }
}
