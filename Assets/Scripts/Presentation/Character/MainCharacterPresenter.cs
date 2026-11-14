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
        private int _replicaIndex;
        private bool _isDisposed;

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
            _replicaTimer?.Kill();
            _replicaTimer = null;
        }

        private void PlayNextReplica()
        {
            if (_isDisposed)
                return;

            if (_replicaIndex >= IntroReplicas.Length)
            {
                _view.HideReplica();
                _sound.PlayAndWait(SoundType.StartGame, StartGuestIntroductions);
                return;
            }

            string replica = IntroReplicas[_replicaIndex];
            _view.ShowReplica(replica);
            _replicaTimer = DOVirtual.DelayedCall(
                    Mathf.Clamp(replica.Length / CharactersPerSecond, MinReplicaDuration, MaxReplicaDuration),
                    CompleteReplica,
                    false)
                .SetUpdate(true);
        }

        private void CompleteReplica()
        {
            _replicaTimer = null;
            _replicaIndex++;
            PlayNextReplica();
        }

        private void StartGuestIntroductions()
        {
            if (!_isDisposed)
                _guestDialogue.StartIntroductions();
        }
    }
}
