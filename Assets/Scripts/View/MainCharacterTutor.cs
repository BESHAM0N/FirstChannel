using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FirstChannel
{
    public sealed class MainCharacterTutor : MonoBehaviour
    {
        private static readonly int IsTalkHash = Animator.StringToHash("IsTalk");

        private const float TalkAnimationDuration = 2f;

        [SerializeField] private GameObject _dialog;
        [SerializeField] private TMP_Text _dialogText;
        [SerializeField] private Animator _animator;

        private DialogueTypewriter _typewriter;
        private Tween _talkTween;

        public void ShowStep(string text, float availableDuration)
        {
            _dialog.SetActive(true);
            _typewriter.Play(text, availableDuration);
            PlayTalkOnce();
        }

        public void PlayIdleAnimation()
        {
            _talkTween?.Kill();
            _talkTween = null;

            if (_animator != null)
                _animator.SetBool(IsTalkHash, false);
        }

        private void Awake()
        {
            _typewriter = new DialogueTypewriter(_dialogText);
            PlayIdleAnimation();
        }

        private void OnDestroy()
        {
            _talkTween?.Kill();
            _typewriter?.Stop();
        }

        private void PlayTalkOnce()
        {
            _talkTween?.Kill();

            if (_animator == null)
                return;

            _animator.SetBool(IsTalkHash, false);
            _animator.Update(0f);
            _animator.SetBool(IsTalkHash, true);

            _talkTween = DOVirtual.DelayedCall(
                    TalkAnimationDuration,
                    PlayIdleAnimation)
                .SetUpdate(true);
        }
    }
}
