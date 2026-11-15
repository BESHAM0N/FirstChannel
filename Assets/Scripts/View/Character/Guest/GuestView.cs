using TMPro;
using UnityEngine;

using DG.Tweening;

namespace FirstChannel
{
    public sealed class GuestView : MonoBehaviour
    {
        private static readonly int IsTalkHash = Animator.StringToHash("IsTalk");
        private static readonly int IsCryHash = Animator.StringToHash("IsCry");

        [SerializeField] private GuestId _id;
        [SerializeField] private Animator _animator;
        [SerializeField] private EmotionBarView _emotionBarView;
        [SerializeField] private GameObject _firstFireAnimation;
        [SerializeField] private GameObject _secondFireAnimation;
        [SerializeField] private GameObject _viewers;
        [SerializeField] private TMP_Text _viewersMultiplierText;
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TMP_Text _dialogText;

        private DialogueTypewriter _typewriter;

        public GuestId Id => _id;

        public void InitializeEmotion(int value)
        {
            _emotionBarView.Initialize(value);
            UpdateFire(value);
        }

        public void UpdateEmotion(int value)
        {
            _emotionBarView.UpdateEmotionBar(value);
            UpdateFire(value);
        }

        public void ShowReplica(string text, float duration)
        {
            _dialogPanel.transform.DOKill();
            _dialogPanel.SetActive(true);
            _dialogPanel.transform.localScale = Vector3.one * 0.9f;
            _typewriter.Play(text, duration);
            _dialogPanel.transform.DOScale(1f, 0.15f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void HideReplica()
        {
            _dialogPanel.transform.DOKill();
            _typewriter.Clear();
            _dialogPanel.SetActive(false);
            PlayIdleAnimation();
        }

        public void PlayTalkAnimation()
        {
            SetAnimationState(isTalking: true, isCrying: false);
        }

        public void PlayCryAnimation()
        {
            SetAnimationState(isTalking: false, isCrying: true);
        }

        public void PlayIdleAnimation()
        {
            SetAnimationState(isTalking: false, isCrying: false);
        }

        public void PlayCardAppliedFeedback()
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.05f, 0.25f, 6, 0.6f)
                .SetUpdate(true);
        }

        public void HideGuest()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            InitializeViewersMultiplierView();
            _typewriter = new DialogueTypewriter(_dialogText);
            HideReplica();
        }

        private void OnDestroy()
        {
            _dialogPanel.transform.DOKill();
            _typewriter?.Stop();
        }

        private void UpdateFire(int value)
        {
            bool isSecondFire = value >= EmotionBar.SecondFireThreshold;
            bool isFirstFire = value >= EmotionBar.FirstFireThreshold && !isSecondFire;

            _firstFireAnimation.SetActive(isFirstFire);
            _secondFireAnimation.SetActive(isSecondFire);

            int viewersMultiplier = EmotionBar.GetViewersMultiplier(value);
            bool hasViewersMultiplier = viewersMultiplier > 1;

            if (_viewersMultiplierText != null)
                _viewersMultiplierText.text = $"x{viewersMultiplier}";

            if (_viewers != null)
                _viewers.SetActive(hasViewersMultiplier);
        }

        private void InitializeViewersMultiplierView()
        {
            if (_viewers == null)
            {
                Transform viewersTransform = transform.Find("Viewers");

                if (viewersTransform != null)
                    _viewers = viewersTransform.gameObject;
            }

            if (_viewersMultiplierText == null && _viewers != null)
                _viewersMultiplierText = _viewers.GetComponentInChildren<TMP_Text>(true);

            if (_viewers != null)
                _viewers.SetActive(false);
        }

        private void SetAnimationState(bool isTalking, bool isCrying)
        {
            if (_animator == null)
                return;

            _animator.SetBool(IsTalkHash, isTalking);
            _animator.SetBool(IsCryHash, isCrying);
        }
    }
}
