using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FirstChannel
{
    public sealed class GameOverView : MonoBehaviour
    {
        private const float CounterDuration = 5f;
        private const float CounterSoundInterval = 0.05f;

        public event Action MainMenuRequested;
        public event Action NewGameRequested;

        [SerializeField] private TMP_Text _textViewersValue;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private GameObject _newRecordText;

        [Inject] private ISoundService _soundService;

        private bool _isInitialized;
        private Tween _counterTween;
        private int _targetViewers;
        private int _displayedViewers;
        private float _nextCounterSoundTime;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            Hide();
        }

        public void Show(int viewers, bool isNewRecord)
        {
            KillCounterTween();

            _targetViewers = Mathf.Max(0, viewers);
            _displayedViewers = 0;
            _nextCounterSoundTime = Time.unscaledTime;

            SetViewersText(0);
            _newRecordText.SetActive(isNewRecord);
            gameObject.SetActive(true);

            if (isNewRecord)
                _soundService.Play(SoundType.NewRecord);

            _counterTween = DOVirtual.Int(0, _targetViewers, CounterDuration, UpdateCounter)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(CompleteCounter);
        }

        public void Hide()
        {
            KillCounterTween();
            gameObject.SetActive(false);
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            KillCounterTween();
            _isInitialized = false;
        }

        private void UpdateCounter(int viewers)
        {
            if (_displayedViewers == viewers)
                return;

            _displayedViewers = viewers;
            SetViewersText(viewers);

            if (viewers >= _targetViewers || Time.unscaledTime < _nextCounterSoundTime)
                return;

            _nextCounterSoundTime = Time.unscaledTime + CounterSoundInterval;
            _soundService.Play(SoundType.Counter);
        }

        private void CompleteCounter()
        {
            _counterTween = null;
            _displayedViewers = _targetViewers;
            SetViewersText(_targetViewers);
            _soundService.Play(SoundType.CounterFull);
        }

        private void SetViewersText(int viewers)
        {
            _textViewersValue.SetText("Вы набрали {0} просмотров", viewers);
        }

        private void KillCounterTween()
        {
            _counterTween?.Kill();
            _counterTween = null;
        }

        private void OnMainMenuClicked()
        {
            _soundService.Play(SoundType.ButtonClick);
            MainMenuRequested?.Invoke();
        }

        private void OnNewGameClicked()
        {
            _soundService.Play(SoundType.ButtonClick);
            NewGameRequested?.Invoke();
        }
    }
}
