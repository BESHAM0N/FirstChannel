using System.Runtime.CompilerServices;
using Zenject;

namespace FirstChannel
{
    public static class ZenjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainer Install(this DiContainer container, Installer installer)
        {
            container.Inject(installer);
            installer.InstallBindings();
            return container;
        }
    }
}