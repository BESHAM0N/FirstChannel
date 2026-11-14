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

        private const string GameSceneName = "GameScene";

        private void OnEnable()
        {
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

        private static void StartGame(GameMode gameMode)
        {
            GameModeSelection.Set(gameMode);
            SceneManager.LoadScene(GameSceneName);
        }
    }
}
