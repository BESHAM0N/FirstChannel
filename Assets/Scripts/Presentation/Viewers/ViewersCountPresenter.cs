using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class ViewersCountPresenter : IInitializable, ITickable, IDisposable
    {
        private const int SoundInterval = 500;

        private readonly int _minViewersPerTick;
        private readonly int _maxViewersPerTick;
        private readonly float _tickInterval;
        private readonly ViewersCount _viewers;
        private readonly GuestRoster _guestRoster;
        private readonly ViewersCountView _view;
        private readonly CardHandPresenter _cardHand;
        private readonly ISoundService _sound;

        private float _elapsedTime;

        public bool IsRunning { get; private set; } = true;

        public ViewersCountPresenter(
            int minViewersPerTick,
            int maxViewersPerTick,
            float tickInterval,
            ViewersCount viewers,
            GuestRoster guestRoster,
            ViewersCountView view,
            CardHandPresenter cardHand,
            ISoundService sound)
        {
            _minViewersPerTick = Mathf.Max(0, minViewersPerTick);
            _maxViewersPerTick = Mathf.Max(_minViewersPerTick, maxViewersPerTick);
            _tickInterval = Mathf.Max(0.01f, tickInterval);
            _viewers = viewers;
            _guestRoster = guestRoster;
            _view = view;
            _cardHand = cardHand;
            _sound = sound;
        }

        public void Initialize()
        {
            _viewers.Changed += OnViewersChanged;
            _cardHand.CardApplied += OnCardApplied;
            _view.Initialize(_viewers.Current);
        }

        public void Tick()
        {
            if (!IsRunning)
                return;

            _elapsedTime += Time.deltaTime;

            while (_elapsedTime >= _tickInterval)
            {
                _elapsedTime -= _tickInterval;
                int viewersToAdd = UnityEngine.Random.Range(
                    _minViewersPerTick,
                    _maxViewersPerTick + 1);
                _viewers.Add(viewersToAdd * _guestRoster.GetViewersMultiplier());
            }
        }

        public void Dispose()
        {
            _viewers.Changed -= OnViewersChanged;
            _cardHand.CardApplied -= OnCardApplied;
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;

            if (!isRunning)
            {
                _elapsedTime = 0f;
                _view.StopAnimations(_viewers.Current);
            }
        }

        private void OnCardApplied(CardPlayResult result)
        {
            _viewers.Add(result.AddedViewers);
        }

        private void OnViewersChanged(int total, int added)
        {
            if (added > 0 && total / SoundInterval > (total - added) / SoundInterval)
                _sound.Play(SoundType.AddViewers);

            _view.UpdateTotal(total);
            _view.ShowAddedViewers(added);
        }
    }
}
