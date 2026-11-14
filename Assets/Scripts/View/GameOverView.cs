using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class GameOverView : MonoBehaviour
    {
        public event Action MainMenuRequested;
        public event Action NewGameRequested;

        [SerializeField] private TMP_Text _textViewersValue;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _newGameButton;

        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            Hide();
        }

        public void Show(int viewers)
        {
            _textViewersValue.SetText("Вы набрали {0} просмотров", viewers);
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

            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            _isInitialized = false;
        }

        private void OnMainMenuClicked()
        {
            MainMenuRequested?.Invoke();
        }

        private void OnNewGameClicked()
        {
            NewGameRequested?.Invoke();
        }
    }
}
