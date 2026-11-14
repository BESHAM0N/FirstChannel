using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class GameCycleInstaller : Installer
    {
        [SerializeField] private GameOverView _gameOverView;
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public override void InstallBindings()
        {
            Container.Bind<GameOverView>().FromInstance(_gameOverView).AsSingle();

            Container
                .BindInterfacesAndSelfTo<GameCycle>()
                .AsSingle()
                .WithArguments(_mainMenuSceneName)
                .NonLazy();
        }
    }
}
