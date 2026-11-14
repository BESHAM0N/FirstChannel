using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class ProjectContextInstaller : MonoInstaller
    {
        [SerializeField] private SoundInstaller _soundInstaller;

        public override void InstallBindings()
        {
            Container.Install(_soundInstaller);
        }
    }
}
