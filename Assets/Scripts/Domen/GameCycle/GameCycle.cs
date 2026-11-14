using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace FirstChannel
{
    public sealed class GameCycle : IInitializable, ILateTickable, IDisposable
    {
        public const string LastViewersPrefsKey = "last_viewers_count";
        public const string BestViewersPrefsKey = "best_viewers_count";

        private readonly Timeline _timeline;
        private readonly TimelinePresenter _timelinePresenter;
        private readonly DeckCard _deck;
        private readonly HandCards _hand;
        private readonly ViewersCount _viewers;
        private readonly ViewersCountPresenter _viewersPresenter;
        private readonly CardHandPresenter _cardHand;
        private readonly GuestDialoguePresenter _guestDialogue;
        private readonly GameOverView _gameOverView;
        private readonly ISoundService _sound;
        private readonly string _mainMenuSceneName;

        private bool _finishRequested;
        private bool _isFinished;
        private bool _cardsExhausted;

        public bool IsFinished => _isFinished;

        public GameCycle(
            Timeline timeline,
            TimelinePresenter timelinePresenter,
            DeckCard deck,
            HandCards hand,
            ViewersCount viewers,
            ViewersCountPresenter viewersPresenter,
            CardHandPresenter cardHand,
            GuestDialoguePresenter guestDialogue,
            GameOverView gameOverView,
            ISoundService sound,
            string mainMenuSceneName)
        {
            _timeline = timeline;
            _timelinePresenter = timelinePresenter;
            _deck = deck;
            _hand = hand;
            _viewers = viewers;
            _viewersPresenter = viewersPresenter;
            _cardHand = cardHand;
            _guestDialogue = guestDialogue;
            _gameOverView = gameOverView;
            _sound = sound;
            _mainMenuSceneName = mainMenuSceneName;
        }

        public void Initialize()
        {
            _timeline.Finished += RequestFinish;
            _deck.Emptied += CheckCardsExhausted;
            _hand.Changed += CheckCardsExhausted;
            _guestDialogue.GuestEmotionDepleted += OnGuestEmotionDepleted;
            _guestDialogue.IntroductionsCompleted += OnIntroductionsCompleted;
            _guestDialogue.CardReactionCompleted += OnCardReactionCompleted;
            _gameOverView.MainMenuRequested += OpenMainMenu;
            _gameOverView.NewGameRequested += RestartGame;

            _gameOverView.Initialize();
            _timelinePresenter.SetRunning(false);
            _viewersPresenter.SetRunning(false);
            _sound.PlayLoop(SoundType.GameBackgroundMusic);

            if (_guestDialogue.AreIntroductionsCompleted)
                OnIntroductionsCompleted();

            CheckCardsExhausted();
        }

        public void LateTick()
        {
            if (_finishRequested &&
                !_isFinished &&
                !_guestDialogue.IsCardReactionPlaying)
            {
                FinishGame();
            }
        }

        public void Dispose()
        {
            _timeline.Finished -= RequestFinish;
            _deck.Emptied -= CheckCardsExhausted;
            _hand.Changed -= CheckCardsExhausted;
            _guestDialogue.GuestEmotionDepleted -= OnGuestEmotionDepleted;
            _guestDialogue.IntroductionsCompleted -= OnIntroductionsCompleted;
            _guestDialogue.CardReactionCompleted -= OnCardReactionCompleted;
            _gameOverView.MainMenuRequested -= OpenMainMenu;
            _gameOverView.NewGameRequested -= RestartGame;
            _gameOverView.Dispose();
        }

        private void OnGuestEmotionDepleted(GuestId guestId)
        {
            RequestFinish();
        }

        private void OnIntroductionsCompleted()
        {
            if (_finishRequested || _isFinished)
                return;

            _timelinePresenter.SetRunning(true);
            _viewersPresenter.SetRunning(true);
        }

        private void CheckCardsExhausted()
        {
            _cardsExhausted = _deck.IsEmpty && _hand.Count == 0;
        }

        private void OnCardReactionCompleted()
        {
            CheckCardsExhausted();

            if (_cardsExhausted)
                RequestFinish();

            if (_finishRequested)
                _cardHand.SetInteractionEnabled(false);
        }

        private void RequestFinish()
        {
            if (_finishRequested || _isFinished)
                return;

            _finishRequested = true;
            _timeline.Stop();
            _timelinePresenter.SetRunning(false);
            _viewersPresenter.SetRunning(false);
            _cardHand.SetInteractionEnabled(false);

            if (!_guestDialogue.IsCardReactionPlaying)
                _guestDialogue.StopDialogue();
        }

        private void FinishGame()
        {
            _isFinished = true;

            int result = _viewers.Current;
            PlayerPrefs.SetInt(LastViewersPrefsKey, result);
            PlayerPrefs.SetInt(
                BestViewersPrefsKey,
                Mathf.Max(result, PlayerPrefs.GetInt(BestViewersPrefsKey, 0)));
            PlayerPrefs.Save();

            _sound.PlayLoop(SoundType.GameOverBackgroundMusic);
            _gameOverView.Show(result);
        }

        private void RestartGame()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void OpenMainMenu()
        {
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
