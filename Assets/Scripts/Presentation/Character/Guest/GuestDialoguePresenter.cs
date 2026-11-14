using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class GuestDialoguePresenter : IInitializable, IDisposable
    {
        public event Action<GuestId> GuestEmotionDepleted;
        public event Action IntroductionsCompleted;
        public event Action CardReactionCompleted;

        private readonly ReplicaListConfig[] _configs;
        private readonly CardHandPresenter _cardHand;
        private readonly ISoundService _sound;
        private readonly GuestRoster _guestRoster;
        private readonly List<GuestView> _guestViews;
        private readonly Dictionary<GuestId, GuestView> _views = new Dictionary<GuestId, GuestView>();

        private Tween _fallbackTimer;
        private GuestView _activeView;
        private int _dialogueSession;
        private int _introductionIndex;
        private bool _isDisposed;
        private bool _isStopped;

        public bool AreIntroductionsCompleted { get; private set; }
        public bool IsCardReactionPlaying { get; private set; }

        public GuestDialoguePresenter(
            ReplicaListConfig[] configs,
            CardHandPresenter cardHand,
            ISoundService sound,
            GuestRoster guestRoster,
            List<GuestView> guestViews)
        {
            _configs = configs;
            _cardHand = cardHand;
            _sound = sound;
            _guestRoster = guestRoster;
            _guestViews = guestViews;
        }

        public void Initialize()
        {
            for (int i = 0; i < _guestViews.Count; i++)
            {
                GuestView view = _guestViews[i];

                if (view == null || _views.ContainsKey(view.Id))
                    continue;

                view.HideReplica();
                _views.Add(view.Id, view);

                if (_guestRoster.TryGetGuest(view.Id, out Guest guest))
                    view.InitializeEmotion(guest.EmotionBar.CurrentValue);
            }

            _cardHand.CardApplied += OnCardApplied;
            StartIntroductions();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _cardHand.CardApplied -= OnCardApplied;
            CancelActiveDialogue();
        }

        public void StopDialogue()
        {
            _isStopped = true;
            CancelActiveDialogue();
        }

        private void StartIntroductions()
        {
            AreIntroductionsCompleted = false;
            _introductionIndex = 0;
            _cardHand.SetInteractionEnabled(false);
            PlayNextIntroduction();
        }

        private void PlayNextIntroduction()
        {
            if (_isDisposed || _isStopped)
                return;

            while (_introductionIndex < _configs.Length)
            {
                ReplicaListConfig config = _configs[_introductionIndex];

                if (config == null ||
                    !_views.TryGetValue(config.GuestId, out GuestView view) ||
                    !HasContent(config.IntroReplica))
                {
                    _introductionIndex++;
                    continue;
                }

                Replica replica = config.IntroReplica;
                int session = ++_dialogueSession;
                _activeView = view;
                view.ShowReplica(replica.Text);

                if (replica.VoiceClip != null)
                {
                    _sound.PlayVoice(replica.VoiceClip, () => CompleteIntroduction(session));
                    return;
                }

                _fallbackTimer = DOVirtual.DelayedCall(
                        replica.Duration,
                        () => CompleteIntroduction(session),
                        false)
                    .SetUpdate(true);
                return;
            }

            AreIntroductionsCompleted = true;
            _cardHand.SetInteractionEnabled(true);
            IntroductionsCompleted?.Invoke();
        }

        private void CompleteIntroduction(int session)
        {
            if (_isDisposed || _isStopped || session != _dialogueSession)
                return;

            _fallbackTimer = null;
            _activeView?.HideReplica();
            _activeView = null;
            _introductionIndex++;
            PlayNextIntroduction();
        }

        private void OnCardApplied(CardPlayResult result)
        {
            if (_isStopped)
                return;

            List<GuestId> depletedGuests = ApplyEmotionChanges(result);
            GuestId dialogueGuestId = result.DialogueGuestId;

            if (!_guestRoster.TryGetGuest(dialogueGuestId, out Guest guest) ||
                !_views.TryGetValue(dialogueGuestId, out GuestView view))
            {
                Debug.LogWarning($"Guest '{dialogueGuestId}' was not configured.");
                CompleteCardReaction(depletedGuests);
                return;
            }

            if (!guest.TryGetReplica(result.Card.CardId, out Replica replica))
            {
                Debug.LogWarning(
                    $"Replica for guest '{dialogueGuestId}' and card '{result.Card.CardId}' was not found.");
                CompleteCardReaction(depletedGuests);
                return;
            }

            CancelActiveDialogue();

            int session = ++_dialogueSession;
            IsCardReactionPlaying = true;
            _activeView = view;
            _cardHand.SetInteractionEnabled(false);
            view.ShowReplica(replica.Text);

            if (result.Card.CardType == CardType.Weeping)
                view.PlayCryAnimation();
            else
                view.PlayTalkAnimation();

            if (replica.VoiceClip != null)
            {
                _sound.PlayVoice(
                    replica.VoiceClip,
                    () => CompleteDialogue(session, depletedGuests));
                return;
            }

            _fallbackTimer = DOVirtual.DelayedCall(
                    replica.Duration,
                    () => CompleteDialogue(session, depletedGuests),
                    false)
                .SetUpdate(true);
        }

        private void CompleteDialogue(int session, List<GuestId> depletedGuests)
        {
            if (_isDisposed || _isStopped || session != _dialogueSession)
                return;

            _fallbackTimer = null;
            _activeView?.HideReplica();
            _activeView = null;
            IsCardReactionPlaying = false;
            _cardHand.SetInteractionEnabled(true);
            CompleteCardReaction(depletedGuests);
        }

        private List<GuestId> ApplyEmotionChanges(CardPlayResult result)
        {
            var depletedGuests = new List<GuestId>(2);
            ApplyEmotionChange(result, GuestId.First, depletedGuests);
            ApplyEmotionChange(result, GuestId.Second, depletedGuests);
            return depletedGuests;
        }

        private void ApplyEmotionChange(
            CardPlayResult result,
            GuestId guestId,
            List<GuestId> depletedGuests)
        {
            if (!result.TryGetEmotionChange(guestId, out int change) ||
                !_guestRoster.TryGetGuest(guestId, out Guest guest))
            {
                return;
            }

            int previousEmotion = guest.EmotionBar.CurrentValue;
            int emotion = guest.ApplyEmotion(change);

            if (_views.TryGetValue(guestId, out GuestView view))
                view.UpdateEmotion(emotion);

            if (previousEmotion > EmotionBar.MinValue && emotion == EmotionBar.MinValue)
                depletedGuests.Add(guestId);
        }

        private void CompleteCardReaction(List<GuestId> depletedGuests)
        {
            for (int i = 0; i < depletedGuests.Count; i++)
            {
                GuestId guestId = depletedGuests[i];

                _sound.Play(SoundType.ExitCharacter);

                if (_views.TryGetValue(guestId, out GuestView view))
                    view.HideGuest();

                GuestEmotionDepleted?.Invoke(guestId);
            }

            CardReactionCompleted?.Invoke();
        }

        private void CancelActiveDialogue()
        {
            _dialogueSession++;
            _fallbackTimer?.Kill();
            _fallbackTimer = null;
            _sound.StopVoice();
            _activeView?.HideReplica();
            _activeView = null;
            IsCardReactionPlaying = false;
        }

        private static bool HasContent(Replica replica)
        {
            return replica.VoiceClip != null || !string.IsNullOrWhiteSpace(replica.Text);
        }
    }
}
