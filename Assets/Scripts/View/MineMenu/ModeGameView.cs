using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FirstChannel
{
    public enum GameMode
    {
        WithCensorship,
        Uncensored
    }

    public static class GameModeSelection
    {
        public static GameMode Current { get; private set; } = GameMode.WithCensorship;

        public static void Set(GameMode gameMode)
        {
            Current = gameMode;
        }
    }

    public sealed class ModeGameView : MonoBehaviour
    {
        [SerializeField] private Button _withCensorshipButton;
        [SerializeField] private Button _uncensoredButton;

        [SerializeField] private GameObject _gameModeWithCensorship1;
        [SerializeField] private GameObject _gameModeWithCensorship2;

        [SerializeField] private GameObject _gameModeUncensored1;
        [SerializeField] private GameObject _gameModeUncensored2;

        [Header("Hover")]
        [SerializeField] private RectTransform _tint;
        [SerializeField, Range(1f, 1.2f)] private float _hoverScale = 1.04f;
        [SerializeField, Min(0f)] private float _hoverDuration = 0.2f;

        private const string GameSceneName = "GameScene";

        private ModeGameButtonHover _withCensorshipHover;
        private ModeGameButtonHover _uncensoredHover;

        private void Awake()
        {
            _withCensorshipHover = ConfigureHover(
                _withCensorshipButton,
                _gameModeWithCensorship1,
                _gameModeWithCensorship2);

            _uncensoredHover = ConfigureHover(
                _uncensoredButton,
                _gameModeUncensored1,
                _gameModeUncensored2);
        }

        private void OnEnable()
        {
            _withCensorshipHover?.ResetState();
            _uncensoredHover?.ResetState();
            _withCensorshipButton.onClick.AddListener(StartWithCensorship);
            _uncensoredButton.onClick.AddListener(StartUncensored);
        }

        private void OnDisable()
        {
            _withCensorshipButton.onClick.RemoveListener(StartWithCensorship);
            _uncensoredButton.onClick.RemoveListener(StartUncensored);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void StartWithCensorship()
        {
            StartGame(GameMode.WithCensorship);
        }

        private void StartUncensored()
        {
            StartGame(GameMode.Uncensored);
        }

        private ModeGameButtonHover ConfigureHover(
            Button button,
            GameObject defaultImage,
            GameObject hoverImage)
        {
            if (button == null || defaultImage == null || hoverImage == null || _tint == null)
            {
                Debug.LogError("ModeGameView hover references are not configured.", this);
                return null;
            }

            Image defaultImageComponent = defaultImage.GetComponent<Image>();
            Image hoverImageComponent = hoverImage.GetComponent<Image>();

            if (defaultImageComponent == null || hoverImageComponent == null)
            {
                Debug.LogError("ModeGameView hover objects must contain an Image component.", this);
                return null;
            }

            ModeGameButtonHover hover = button.GetComponent<ModeGameButtonHover>();

            if (hover == null)
                hover = button.gameObject.AddComponent<ModeGameButtonHover>();

            hover.Initialize(
                button,
                _tint,
                defaultImageComponent,
                hoverImageComponent,
                _hoverScale,
                _hoverDuration);

            return hover;
        }

        private static void StartGame(GameMode gameMode)
        {
            GameModeSelection.Set(gameMode);
            SceneManager.LoadScene(GameSceneName);
        }
    }
}
