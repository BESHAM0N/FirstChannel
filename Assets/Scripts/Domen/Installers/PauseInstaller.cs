using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class PauseInstaller : Installer
    {
        [SerializeField] private PauseView _view;
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public override void InstallBindings()
        {
            Container.Bind<PauseView>().FromInstance(_view).AsSingle();
            Container.Bind<SettingPanelView>().FromInstance(_view.SettingPanelView).AsSingle();
            Container
                .BindInterfacesAndSelfTo<PausePresenter>()
                .AsSingle()
                .WithArguments(_mainMenuSceneName)
                .NonLazy();
        }
    }
}
