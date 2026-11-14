using System;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class ViewersCountPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly int _minViewersPerTick;
        private readonly int _maxViewersPerTick;
        private readonly float _tickInterval;
        private readonly ViewersCount _viewers;
        private readonly ViewersCountView _view;
        private readonly CardHandPresenter _cardHand;

        private float _elapsedTime;

        public bool IsRunning { get; private set; } = true;

        public ViewersCountPresenter(
            int minViewersPerTick,
            int maxViewersPerTick,
            float tickInterval,
            ViewersCount viewers,
            ViewersCountView view,
            CardHandPresenter cardHand)
        {
            _minViewersPerTick = Mathf.Max(0, minViewersPerTick);
            _maxViewersPerTick = Mathf.Max(_minViewersPerTick, maxViewersPerTick);
            _tickInterval = Mathf.Max(0.01f, tickInterval);
            _viewers = viewers;
            _view = view;
            _cardHand = cardHand;
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

            _elapsedTime += Time.unscaledDeltaTime;

            while (_elapsedTime >= _tickInterval)
            {
                _elapsedTime -= _tickInterval;
                int viewersToAdd = UnityEngine.Random.Range(
                    _minViewersPerTick,
                    _maxViewersPerTick + 1);
                _viewers.Add(viewersToAdd);
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
        }

        private void OnCardApplied(Card card, GuestId guestId)
        {
            _viewers.Add(card.AddViewers);
        }

        private void OnViewersChanged(int total, int added)
        {
            _view.UpdateTotal(total);
            _view.ShowAddedViewers(added);
        }
    }
}
