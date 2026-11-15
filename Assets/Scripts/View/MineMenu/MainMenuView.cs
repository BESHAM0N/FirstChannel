using DG.Tweening;
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

        [Header("Panels")]
        [SerializeField] private SettingPanelView _settingPanelView;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _authorsPanel;
        [SerializeField] private Button _authorsCloseButton;
        [SerializeField, Min(0f)] private float _panelAnimationDuration = 0.25f;
        [SerializeField, Range(0.5f, 1f)] private float _hiddenPanelScale = 0.92f;

        [Inject] private ISoundService _soundService;

        private RectTransform _settingsTransform;
        private RectTransform _authorsTransform;
        private CanvasGroup _settingsCanvasGroup;
        private CanvasGroup _authorsCanvasGroup;
        private Vector3 _settingsVisibleScale;
        private Vector3 _authorsVisibleScale;
        private Sequence _settingsTween;
        private Sequence _authorsTween;

        private void Awake()
        {
            _settingPanelView.Initialize();

            InitializePanel(
                _settingsPanel,
                out _settingsTransform,
                out _settingsCanvasGroup,
                out _settingsVisibleScale);

            InitializePanel(
                _authorsPanel,
                out _authorsTransform,
                out _authorsCanvasGroup,
                out _authorsVisibleScale);
        }

        private void Start()
        {
            _soundService.PlayLoop(SoundType.MainMenuBackgroundMusic);
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OpenModeSelection);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ExitGame);
            _authorsButton.onClick.AddListener(OpenAuthors);
            _authorsCloseButton.onClick.AddListener(CloseAuthors);

            _settingPanelView.OnBackButtonClicked += CloseSettings;
            _settingPanelView.MasterVolumeChanged += ChangeMasterVolume;
            _settingPanelView.MusicVolumeChanged += ChangeMusicVolume;
            _settingPanelView.EffectVolumeChanged += ChangeEffectVolume;
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OpenModeSelection);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ExitGame);
            _authorsButton.onClick.RemoveListener(OpenAuthors);
            _authorsCloseButton.onClick.RemoveListener(CloseAuthors);

            _settingPanelView.OnBackButtonClicked -= CloseSettings;
            _settingPanelView.MasterVolumeChanged -= ChangeMasterVolume;
            _settingPanelView.MusicVolumeChanged -= ChangeMusicVolume;
            _settingPanelView.EffectVolumeChanged -= ChangeEffectVolume;
        }

        private void OpenModeSelection()
        {
            PlayButtonClick();
            _modeGameView.Show();
        }

        private void OpenSettings()
        {
            PlayButtonClick();
            _settingPanelView.SetVolumeValues(
                _soundService.MasterVolume,
                _soundService.MusicVolume,
                _soundService.EffectVolume);

            ShowPanel(
                _settingsPanel,
                _settingsTransform,
                _settingsCanvasGroup,
                _settingsVisibleScale,
                ref _settingsTween);
        }

        private void CloseSettings()
        {
            PlayButtonClick();
            HidePanel(
                _settingsPanel,
                _settingsTransform,
                _settingsCanvasGroup,
                _settingsVisibleScale,
                ref _settingsTween);
        }

        private void OpenAuthors()
        {
            PlayButtonClick();
            ShowPanel(
                _authorsPanel,
                _authorsTransform,
                _authorsCanvasGroup,
                _authorsVisibleScale,
                ref _authorsTween);
        }

        private void CloseAuthors()
        {
            PlayButtonClick();
            HidePanel(
                _authorsPanel,
                _authorsTransform,
                _authorsCanvasGroup,
                _authorsVisibleScale,
                ref _authorsTween);
        }

        private void ExitGame()
        {
            _soundService.PlayAndWait(SoundType.ButtonClick, QuitApplication);
        }

        private void ChangeMasterVolume(float value)
        {
            _soundService.ChangeMasterVolume(value);
        }

        private void ChangeMusicVolume(float value)
        {
            _soundService.ChangeMusicVolume(value);
        }

        private void ChangeEffectVolume(float value)
        {
            _soundService.ChangeEffectVolume(value);
        }

        private void PlayButtonClick()
        {
            _soundService.Play(SoundType.ButtonClick);
        }

        private void InitializePanel(
            GameObject panel,
            out RectTransform panelTransform,
            out CanvasGroup canvasGroup,
            out Vector3 visibleScale)
        {
            panelTransform = panel.transform as RectTransform;
            canvasGroup = panel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            visibleScale = panelTransform.localScale;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            panelTransform.localScale = visibleScale * _hiddenPanelScale;
            panel.SetActive(false);
        }

        private void ShowPanel(
            GameObject panel,
            RectTransform panelTransform,
            CanvasGroup canvasGroup,
            Vector3 visibleScale,
            ref Sequence tween)
        {
            tween?.Kill();
            panel.SetActive(true);
            panelTransform.localScale = visibleScale * _hiddenPanelScale;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            tween = DOTween.Sequence()
                .Join(canvasGroup.DOFade(1f, _panelAnimationDuration))
                .Join(panelTransform
                    .DOScale(visibleScale, _panelAnimationDuration)
                    .SetEase(Ease.OutBack))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }

        private void HidePanel(
            GameObject panel,
            RectTransform panelTransform,
            CanvasGroup canvasGroup,
            Vector3 visibleScale,
            ref Sequence tween)
        {
            if (!panel.activeSelf)
                return;

            tween?.Kill();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            tween = DOTween.Sequence()
                .Join(canvasGroup.DOFade(0f, _panelAnimationDuration))
                .Join(panelTransform
                    .DOScale(visibleScale * _hiddenPanelScale, _panelAnimationDuration)
                    .SetEase(Ease.InBack))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = false;
                    panel.SetActive(false);
                });
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            _settingsTween?.Kill();
            _authorsTween?.Kill();
        }
    }
}
