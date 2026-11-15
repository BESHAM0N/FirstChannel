using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class MainCharacterPresenter : IInitializable, IDisposable
    {
        private const string DefaultPlayerName = "Игрок";
        private const float CharactersPerSecond = 25f;
        private const float MinReplicaDuration = 3f;
        private const float MaxReplicaDuration = 8f;
        private const float CardTalkDuration = 2f;
        private const float StartGameVolumeMultiplier = 0.6f;

        private static readonly string[] IntroReplicas =
        {
            "За или против? Согласны или отрицаете? В нашей студии эти вопросы звучат постоянно. " +
            "Сегодня, как и всегда, мы затронем волнующую всех тему.",
            "Нельзя остаться в стороне, когда обсуждение касается кулинарии, а особенно если " +
            "рассматривается интересный рецепт блинов."
        };

        private readonly CharacterView _view;
        private readonly GuestDialoguePresenter _guestDialogue;
        private readonly CardHandPresenter _cardHand;
        private readonly ISoundService _sound;

        private Tween _replicaTimer;
        private Tween _cardTalkTimer;
        private int _replicaIndex;
        private bool _isDisposed;
        private bool _isIntroVoicePlaying;

        public MainCharacterPresenter(
            CharacterView view,
            GuestDialoguePresenter guestDialogue,
            CardHandPresenter cardHand,
            ISoundService sound)
        {
            _view = view;
            _guestDialogue = guestDialogue;
            _cardHand = cardHand;
            _sound = sound;
        }

        public void Initialize()
        {
            _cardHand.CardApplied += OnCardApplied;
            _cardHand.SetInteractionEnabled(false);

            string playerName = PlayerPrefs.GetString(
                LeaderboardView.PlayerNamePrefsKey,
                DefaultPlayerName);

            _view.Initialize(string.IsNullOrWhiteSpace(playerName) ? DefaultPlayerName : playerName);
            PlayNextReplica();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _cardHand.CardApplied -= OnCardApplied;
            _replicaTimer?.Kill();
            _replicaTimer = null;

            if (_isIntroVoicePlaying)
            {
                _isIntroVoicePlaying = false;
                _sound.StopVoice();
            }

            _cardTalkTimer?.Kill();
            _cardTalkTimer = null;
            _view.PlayIdleAnimation();
        }

        private void PlayNextReplica()
        {
            if (_isDisposed)
                return;

            if (_replicaIndex >= IntroReplicas.Length)
            {
                _view.HideReplica();
                _view.PlayIdleAnimation();
                _sound.PlayAndWait(
                    SoundType.StartGame,
                    StartGuestIntroductions,
                    StartGameVolumeMultiplier);
                return;
            }

            string replica = IntroReplicas[_replicaIndex];
            AudioClip voiceClip = _view.GetIntroVoiceClip(_replicaIndex);
            float duration = voiceClip != null
                ? Mathf.Max(0.1f, voiceClip.length)
                : Mathf.Clamp(
                    replica.Length / CharactersPerSecond,
                    MinReplicaDuration,
                    MaxReplicaDuration);

            _view.PlayTalkAnimation();
            _view.ShowReplica(replica, duration);

            if (voiceClip != null)
            {
                _isIntroVoicePlaying = true;
                _sound.PlayVoice(voiceClip, CompleteReplica);
                return;
            }

            _replicaTimer = DOVirtual.DelayedCall(
                    duration,
                    CompleteReplica,
                    false);
        }

        private void CompleteReplica()
        {
            if (_isDisposed)
                return;

            _isIntroVoicePlaying = false;
            _replicaTimer = null;
            _replicaIndex++;
            PlayNextReplica();
        }

        private void OnCardApplied(CardPlayResult _)
        {
            _cardTalkTimer?.Kill();
            _view.PlayTalkAnimation();
            _cardTalkTimer = DOVirtual.DelayedCall(CardTalkDuration, CompleteCardTalk, false);
        }

        private void CompleteCardTalk()
        {
            _cardTalkTimer = null;
            _view.PlayIdleAnimation();
        }

        private void StartGuestIntroductions()
        {
            if (_isDisposed)
                return;

            _sound.PlayLoop(SoundType.GameBackgroundMusic);
            _guestDialogue.StartIntroductions();
        }
    }
}
