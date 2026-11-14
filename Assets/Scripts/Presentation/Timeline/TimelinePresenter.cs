using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class TimelinePresenter : IInitializable, ITickable, IDisposable
    {
        private readonly Timeline _timeline;
        private readonly TimelineView _view;

        public bool IsRunning { get; private set; } = true;

        public TimelinePresenter(Timeline timeline, TimelineView view)
        {
            _timeline = timeline;
            _view = view;
        }

        public void Initialize()
        {
            _timeline.Updated += UpdateView;
            _view.Initialize(_timeline.Progress, FormatTime(_timeline.ElapsedSeconds));
        }

        public void Tick()
        {
            if (!IsRunning)
                return;

            _timeline.Advance(Time.unscaledDeltaTime);
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public void Dispose()
        {
            _timeline.Updated -= UpdateView;
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
