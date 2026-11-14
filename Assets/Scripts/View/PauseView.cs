using System;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class PauseView : MonoBehaviour
    {
        public event Action OpenRequested;
        public event Action CloseRequested;
        public event Action MainMenuRequested;

        [SerializeField] private Button _menuButton;
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private SettingPanelView _settingPanelView;
        [SerializeField] private Button _mainMenuButton;

        private bool _isInitialized;

        public SettingPanelView SettingPanelView => _settingPanelView;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
            _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

            if (_closeButtons != null)
            {
                for (int i = 0; i < _closeButtons.Length; i++)
                {
                    if (_closeButtons[i] != null)
                        _closeButtons[i].onClick.AddListener(OnCloseButtonClicked);
                }
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
            _mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);

            if (_closeButtons != null)
            {
                for (int i = 0; i < _closeButtons.Length; i++)
                {
                    if (_closeButtons[i] != null)
                        _closeButtons[i].onClick.RemoveListener(OnCloseButtonClicked);
                }
            }

            _isInitialized = false;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnMenuButtonClicked()
        {
            OpenRequested?.Invoke();
        }

        private void OnCloseButtonClicked()
        {
            CloseRequested?.Invoke();
        }

        private void OnMainMenuButtonClicked()
        {
            MainMenuRequested?.Invoke();
        }
    }
}
