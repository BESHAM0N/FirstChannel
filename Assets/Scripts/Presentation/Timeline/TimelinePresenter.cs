using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class TimelinePresenter : IInitializable, ITickable, IDisposable
    {
        private readonly Timeline _timeline;
        private readonly TimelineView _view;
        private readonly ISoundService _sound;

        public bool IsRunning { get; private set; } = true;

        public TimelinePresenter(
            Timeline timeline,
            TimelineView view,
            ISoundService sound)
        {
            _timeline = timeline;
            _view = view;
            _sound = sound;
        }

        public void Initialize()
        {
            _timeline.Updated += UpdateView;
            _view.MasterVolumeChanged += _sound.ChangeMasterVolume;
            _sound.MasterVolumeChanged += _view.SetMasterVolume;
            _view.Initialize(
                _timeline.Progress,
                FormatTime(_timeline.ElapsedSeconds),
                _sound.MasterVolume);
        }

        public void Tick()
        {
            if (!IsRunning)
                return;

            _timeline.Advance(Time.deltaTime);
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public void Dispose()
        {
            _timeline.Updated -= UpdateView;
            _view.MasterVolumeChanged -= _sound.ChangeMasterVolume;
            _sound.MasterVolumeChanged -= _view.SetMasterVolume;
            _view.Dispose();
        }

        private void UpdateView()
        {
            _view.UpdateTimeline(_timeline.Progress, FormatTime(_timeline.ElapsedSeconds));
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;

            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
