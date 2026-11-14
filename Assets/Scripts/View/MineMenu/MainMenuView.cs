using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FirstChannel
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private ModeGameView _modeGameView;

        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _authorsButton;

        [Inject] private ISoundService _soundService;

        private void Start()
        {
            _soundService.PlayLoop(SoundType.MainMenuBackgroundMusic);
        }

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
