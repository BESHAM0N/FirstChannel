using DG.Tweening;
using FirstChannel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Tools
{
    public sealed class ScaleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale")]
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _duration = 0.15f;
        [SerializeField] private Ease _ease = Ease.OutBack;
        [SerializeField] private Button _button;
        [SerializeField] private bool _isSimpleButton;
        private RectTransform _rectTransform;
        private Vector3 _defaultScale;
        private Tween _scaleTween;

        [Inject] private ISoundService _soundService;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultScale = _rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _soundService.Play(SoundType.ButtonHover);
            ScaleTo(_defaultScale * _hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ScaleTo(_defaultScale);
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(Click);
        }

        private void ScaleTo(Vector3 targetScale)
        {
            _scaleTween?.Kill();

            _scaleTween = _rectTransform
                .DOScale(targetScale, _duration)
                .SetEase(_ease)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _scaleTween = null;

            if (_rectTransform != null)
                _rectTransform.localScale = _defaultScale;

            if (_button != null)
                _button.onClick.RemoveListener(Click);
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
        }

        private void Click()
        {
            if (_isSimpleButton)
                _soundService.Play(SoundType.ButtonClick);
        }
    }
}