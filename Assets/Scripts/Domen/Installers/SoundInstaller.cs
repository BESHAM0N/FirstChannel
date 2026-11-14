using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class SoundInstaller : Installer
    {
        [SerializeField] private SoundConfig _config;

        public override void InstallBindings()
        {
            Container.Bind<SoundConfig>().FromInstance(_config).AsSingle();
            Container.Bind<SoundWorld>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SoundService>().AsSingle().NonLazy();
        }
    }
}
