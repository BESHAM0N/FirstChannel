using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class ModeGameButtonHover : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private Button _button;
        private RectTransform _buttonTransform;
        private RectTransform _tint;
        private Image _defaultImage;
        private Image _hoverImage;
        private Vector3 _initialScale;
        private int _initialSiblingIndex;
        private float _defaultImageAlpha;
        private float _hoverImageAlpha;
        private float _hoverScale;
        private float _duration;
        private bool _isHovered;
        private Tween _scaleTween;
        private Sequence _imageTween;

        public void Initialize(
            Button button,
            RectTransform tint,
            Image defaultImage,
            Image hoverImage,
            float hoverScale,
            float duration)
        {
            _button = button;
            _buttonTransform = button.transform as RectTransform;
            _tint = tint;
            _defaultImage = defaultImage;
            _hoverImage = hoverImage;
            _initialScale = _buttonTransform.localScale;
            _initialSiblingIndex = _buttonTransform.GetSiblingIndex();
            _defaultImageAlpha = defaultImage.color.a;
            _hoverImageAlpha = hoverImage.color.a;
            _hoverScale = hoverScale;
            _duration = duration;

            ResetState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button == null || !_button.IsInteractable())
                return;

            _isHovered = true;
            MoveAboveTint();
            AnimateImages(true);
            AnimateScale(_initialScale * _hoverScale, Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_buttonTransform == null)
                return;

            _isHovered = false;
            _buttonTransform.SetSiblingIndex(_initialSiblingIndex);
            AnimateImages(false);
            AnimateScale(_initialScale, Ease.OutCubic);
        }

        public void ResetState()
        {
            KillTweens();
            _isHovered = false;
            _buttonTransform.SetSiblingIndex(_initialSiblingIndex);
            _buttonTransform.localScale = _initialScale;
            SetAlpha(_defaultImage, _defaultImageAlpha);
            SetAlpha(_hoverImage, 0f);
            _defaultImage.gameObject.SetActive(true);
            _hoverImage.gameObject.SetActive(false);
        }

        private void MoveAboveTint()
        {
            int siblingIndex = Mathf.Min(
                _tint.GetSiblingIndex() + 1,
                _buttonTransform.parent.childCount - 1);

            _buttonTransform.SetSiblingIndex(siblingIndex);
        }

        private void AnimateImages(bool showHover)
        {
            _imageTween?.Kill();
            _defaultImage.gameObject.SetActive(true);
            _hoverImage.gameObject.SetActive(true);

            _imageTween = DOTween.Sequence()
                .Join(_defaultImage.DOFade(showHover ? 0f : _defaultImageAlpha, _duration))
                .Join(_hoverImage.DOFade(showHover ? _hoverImageAlpha : 0f, _duration))
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (_isHovered == showHover)
                    {
                        _defaultImage.gameObject.SetActive(!showHover);
                        _hoverImage.gameObject.SetActive(showHover);
                    }
                });
        }

        private void AnimateScale(Vector3 targetScale, Ease ease)
        {
            _scaleTween?.Kill();
            _scaleTween = _buttonTransform
                .DOScale(targetScale, _duration)
                .SetEase(ease)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            KillTweens();
            _isHovered = false;
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _scaleTween?.Kill();
            _imageTween?.Kill();
            _scaleTween = null;
            _imageTween = null;
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
