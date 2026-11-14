using DG.Tweening;
using UnityEngine;

namespace Tools
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIPanelTweenView : MonoBehaviour
    {
        [Header("Root")] [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")] [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InBack;

        [Header("Scale")] [SerializeField] private bool _useScale = true;
        [SerializeField] private Vector3 _hiddenScale = new(0.9f, 0.9f, 0.9f);
        [SerializeField] private Vector3 _visibleScale = Vector3.one;

        [Header("Fade")] [SerializeField] private bool _useFade = true;
        [SerializeField] private float _hiddenAlpha = 0f;
        [SerializeField] private float _visibleAlpha = 1f;

        [Header("Slide")] [SerializeField] private bool _useSlide;
        [SerializeField] private Vector2 _hiddenOffset = new(0f, -40f);

        [Header("State")] [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private bool _disableObjectWhenHidden = true;

        private Sequence _sequence;
        private Vector2 _visibleAnchoredPosition;
        private bool _isVisible;
        private bool _isInitialized;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            InitializeIfNeeded();

            if (_hideOnAwake && !_isVisible)
                HideInstant();
            else if (!_hideOnAwake && !_isVisible)
                ShowInstant();
        }

        public void Show()
        {
            InitializeIfNeeded();

            if (_isVisible)
                return;

            _isVisible = true;

            gameObject.SetActive(true);

            _sequence?.Kill();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_useFade)
                _canvasGroup.alpha = _hiddenAlpha;

            if (_useScale)
                _root.localScale = _hiddenScale;

            if (_useSlide)
                _root.anchoredPosition = _visibleAnchoredPosition + _hiddenOffset;

            _sequence = DOTween.Sequence();

            if (_useFade)
                _sequence.Join(_canvasGroup.DOFade(_visibleAlpha, _duration));

            if (_useScale)
                _sequence.Join(_root.DOScale(_visibleScale, _duration).SetEase(_showEase));

            if (_useSlide)
                _sequence.Join(_root.DOAnchorPos(_visibleAnchoredPosition, _duration).SetEase(_showEase));

            _sequence
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                });
        }

        public void Hide()
        {
            InitializeIfNeeded();

            if (!_isVisible)
                return;

            _isVisible = false;

            _sequence?.Kill();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _sequence = DOTween.Sequence();

            if (_useFade)
                _sequence.Join(_canvasGroup.DOFade(_hiddenAlpha, _duration));

            if (_useScale)
                _sequence.Join(_root.DOScale(_hiddenScale, _duration).SetEase(_hideEase));

            if (_useSlide)
                _sequence.Join(
                    _root.DOAnchorPos(_visibleAnchoredPosition + _hiddenOffset, _duration).SetEase(_hideEase));

            _sequence
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (_disableObjectWhenHidden)
                        gameObject.SetActive(false);
                });
        }

        public void Toggle()
        {
            InitializeIfNeeded();

            if (_isVisible)
                Hide();
            else
                Show();
        }

        public void ShowInstant()
        {
            InitializeIfNeeded();

            _sequence?.Kill();

            _isVisible = true;

            gameObject.SetActive(true);

            if (_useFade)
                _canvasGroup.alpha = _visibleAlpha;

            if (_useScale)
                _root.localScale = _visibleScale;

            if (_useSlide)
                _root.anchoredPosition = _visibleAnchoredPosition;

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void HideInstant()
        {
            InitializeIfNeeded();

            _sequence?.Kill();

            _isVisible = false;

            if (_useFade)
                _canvasGroup.alpha = _hiddenAlpha;

            if (_useScale)
                _root.localScale = _hiddenScale;

            if (_useSlide)
                _root.anchoredPosition = _visibleAnchoredPosition + _hiddenOffset;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_disableObjectWhenHidden)
                gameObject.SetActive(false);
        }

        private void InitializeIfNeeded()
        {
            if (_isInitialized)
                return;

            if (_root == null)
                _root = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_root != null)
                _visibleAnchoredPosition = _root.anchoredPosition;

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}