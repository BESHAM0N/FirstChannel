using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FirstChannel
{
    public sealed class CharacterView : MonoBehaviour
    {
        private static readonly int IsTalkHash = Animator.StringToHash("IsTalk");

        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Animator _animator;

        private DialogueTypewriter _typewriter;

        public void Initialize(string playerName)
        {
            gameObject.SetActive(true);
            _name.text = playerName;
            HideReplica();
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
        }

        public void PlayTalkAnimation()
        {
            SetTalkState(true);
        }

        public void PlayIdleAnimation()
        {
            SetTalkState(false);
        }

        private void Awake()
        {
            _typewriter = new DialogueTypewriter(_text);
        }

        private void OnDestroy()
        {
            _dialogPanel.transform.DOKill();
            _typewriter?.Stop();
        }

        private void SetTalkState(bool isTalking)
        {
            if (_animator != null)
                _animator.SetBool(IsTalkHash, isTalking);
        }
    }
}
