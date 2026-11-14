using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    [Serializable]
    public sealed class TimelineInstaller : Installer
    {
        [SerializeField] private TimelineView _timelineView;
        [SerializeField, Min(1f)] private float _durationSeconds = Timeline.DefaultDurationSeconds;
        
        public override void InstallBindings()
        {
            Container
                .Bind<TimelineView>()
                .FromInstance(_timelineView)
                .AsSingle();

            Container
                .Bind<Timeline>()
                .AsSingle()
                .WithArguments(_durationSeconds);

            Container
                .BindInterfacesAndSelfTo<TimelinePresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}
