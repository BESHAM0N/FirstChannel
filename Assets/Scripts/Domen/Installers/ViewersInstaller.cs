using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class ViewersInstaller : Installer
    {
        [SerializeField] private ViewersCountView _view;
        [SerializeField, Min(0)] private int _minViewersPerTick = 1;
        [SerializeField, Min(0)] private int _maxViewersPerTick = 5;
        [SerializeField, Min(0.01f)] private float _tickInterval = 1f;

        public override void InstallBindings()
        {
            Container.Bind<ViewersCountView>().FromInstance(_view).AsSingle();
            Container.Bind<ViewersCount>().AsSingle();

            Container
                .BindInterfacesAndSelfTo<ViewersCountPresenter>()
                .AsSingle()
                .WithArguments(_minViewersPerTick, _maxViewersPerTick, _tickInterval)
                .NonLazy();
        }
    }
}
