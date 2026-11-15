using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class GuestDialoguePresenter : IInitializable, IDisposable
    {
        public event Action<GuestId> GuestPatienceLimitReached;
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
        private bool _introductionsStarted;

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
            _cardHand.SetInteractionEnabled(false);
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

        public void StartIntroductions()
        {
            if (_isDisposed || _isStopped || _introductionsStarted)
                return;

            _introductionsStarted = true;
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
                view.ShowReplica(replica.Text, replica.Duration);

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

            List<GuestId> departingGuests = ApplyPatienceChanges(result);
            GuestId dialogueGuestId = result.DialogueGuestId;

            if (!_guestRoster.TryGetGuest(dialogueGuestId, out Guest guest) ||
                !_views.TryGetValue(dialogueGuestId, out GuestView view))
            {
                Debug.LogWarning($"Guest '{dialogueGuestId}' was not configured.");
                CompleteCardReaction(departingGuests);
                return;
            }

            if (!guest.TryGetReplica(result.Card.CardId, out Replica replica))
            {
                Debug.LogWarning(
                    $"Replica for guest '{dialogueGuestId}' and card '{result.Card.CardId}' was not found.");
                CompleteCardReaction(departingGuests);
                return;
            }

            CancelActiveDialogue();

            int session = ++_dialogueSession;
            IsCardReactionPlaying = true;
            _activeView = view;
            _cardHand.SetInteractionEnabled(false);
            view.ShowReplica(replica.Text, replica.Duration);

            if (result.Card.CardType == CardType.Weeping)
                view.PlayCryAnimation();
            else
                view.PlayTalkAnimation();

            if (replica.VoiceClip != null)
            {
                _sound.PlayVoice(
                    replica.VoiceClip,
                    () => CompleteDialogue(session, departingGuests));
                return;
            }

            _fallbackTimer = DOVirtual.DelayedCall(
                    replica.Duration,
                    () => CompleteDialogue(session, departingGuests),
                    false)
                .SetUpdate(true);
        }

        private void CompleteDialogue(int session, List<GuestId> departingGuests)
        {
            if (_isDisposed || _isStopped || session != _dialogueSession)
                return;

            _fallbackTimer = null;
            _activeView?.HideReplica();
            _activeView = null;
            IsCardReactionPlaying = false;
            _cardHand.SetInteractionEnabled(true);
            CompleteCardReaction(departingGuests);
        }

        private List<GuestId> ApplyPatienceChanges(CardPlayResult result)
        {
            var departingGuests = new List<GuestId>(2);
            ApplyPatienceChange(result, GuestId.First, departingGuests);
            ApplyPatienceChange(result, GuestId.Second, departingGuests);
            return departingGuests;
        }

        private void ApplyPatienceChange(
            CardPlayResult result,
            GuestId guestId,
            List<GuestId> departingGuests)
        {
            if (!result.TryGetEmotionChange(guestId, out int change) ||
                !_guestRoster.TryGetGuest(guestId, out Guest guest))
            {
                return;
            }

            int previousPatience = guest.EmotionBar.CurrentValue;
            int patience = guest.ApplyEmotion(change);

            if (_views.TryGetValue(guestId, out GuestView view))
                view.UpdateEmotion(patience);

            if (previousPatience < EmotionBar.MaxValue && patience >= EmotionBar.MaxValue)
                departingGuests.Add(guestId);
        }

        private void CompleteCardReaction(List<GuestId> departingGuests)
        {
            for (int i = 0; i < departingGuests.Count; i++)
            {
                GuestId guestId = departingGuests[i];

                _sound.Play(SoundType.ExitCharacter);

                if (_views.TryGetValue(guestId, out GuestView view))
                    view.HideGuest();

                GuestPatienceLimitReached?.Invoke(guestId);
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
