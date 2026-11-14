using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private ModeGameView _modeGameView;

        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _authorsButton;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(_modeGameView.Show);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(_modeGameView.Show);
        }
    }
}
