using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class GameContextInstaller : MonoInstaller
    {
        [SerializeField] private TimelineInstaller _timelineInstaller;
        [SerializeField] private CardInstaller _cardInstaller;
        [SerializeField] private PauseInstaller _pauseInstaller;
        [SerializeField] private GuestInstaller _guestInstaller;
        [SerializeField] private ViewersInstaller _viewersInstaller;
        [SerializeField] private GameCycleInstaller _gameCycleInstaller;

        public override void InstallBindings()
        {
            Container.Install(_timelineInstaller);
            Container.Install(_cardInstaller);
            Container.Install(_pauseInstaller);
            Container.Install(_guestInstaller);
            Container.Install(_viewersInstaller);
            Container.Install(_gameCycleInstaller);
        }
    }
}
