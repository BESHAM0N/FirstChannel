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
        private readonly List<GuestView> _guestViews;
        private readonly Dictionary<GuestId, Guest> _guests = new Dictionary<GuestId, Guest>();
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
            List<GuestView> guestViews)
        {
            _configs = configs;
            _cardHand = cardHand;
            _sound = sound;
            _guestViews = guestViews;
        }

        public void Initialize()
        {
            for (int i = 0; i < _configs.Length; i++)
            {
                ReplicaListConfig config = _configs[i];

                if (config != null && !_guests.ContainsKey(config.GuestId))
                    _guests.Add(config.GuestId, new Guest(config));
            }

            for (int i = 0; i < _guestViews.Count; i++)
            {
                GuestView view = _guestViews[i];

                if (view == null || _views.ContainsKey(view.Id))
                    continue;

                view.HideReplica();
                _views.Add(view.Id, view);

                if (_guests.TryGetValue(view.Id, out Guest guest))
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

        private void OnCardApplied(Card card, GuestId guestId)
        {
            if (_isStopped)
                return;

            if (!_guests.TryGetValue(guestId, out Guest guest) ||
                !_views.TryGetValue(guestId, out GuestView view))
            {
                Debug.LogWarning($"Guest '{guestId}' was not configured.");
                CardReactionCompleted?.Invoke();
                return;
            }

            int emotion = guest.ApplyEmotion(card);
            view.UpdateEmotion(emotion);
            bool isDepleted = emotion == EmotionBar.MinValue;

            if (!guest.TryGetReplica(card.CardId, out Replica replica))
            {
                Debug.LogWarning($"Replica for guest '{guestId}' and card '{card.CardId}' was not found.");
                CompleteCardReaction(guestId, isDepleted);
                return;
            }

            CancelActiveDialogue();

            int session = ++_dialogueSession;
            IsCardReactionPlaying = true;
            _activeView = view;
            _cardHand.SetInteractionEnabled(false);
            view.ShowReplica(replica.Text);

            if (replica.VoiceClip != null)
            {
                _sound.PlayVoice(
                    replica.VoiceClip,
                    () => CompleteDialogue(session, guestId, isDepleted));
                return;
            }

            _fallbackTimer = DOVirtual.DelayedCall(
                    replica.Duration,
                    () => CompleteDialogue(session, guestId, isDepleted),
                    false)
                .SetUpdate(true);
        }

        private void CompleteDialogue(int session, GuestId guestId, bool isDepleted)
        {
            if (_isDisposed || _isStopped || session != _dialogueSession)
                return;

            _fallbackTimer = null;
            _activeView?.HideReplica();
            _activeView = null;
            IsCardReactionPlaying = false;
            _cardHand.SetInteractionEnabled(true);
            CompleteCardReaction(guestId, isDepleted);
        }

        private void CompleteCardReaction(GuestId guestId, bool isDepleted)
        {
            if (isDepleted)
                GuestEmotionDepleted?.Invoke(guestId);

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
