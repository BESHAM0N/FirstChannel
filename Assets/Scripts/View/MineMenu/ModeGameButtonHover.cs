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
        private int _defaultImageSiblingIndex;
        private int _hoverImageSiblingIndex;
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
            _defaultImageSiblingIndex = defaultImage.transform.GetSiblingIndex();
            _hoverImageSiblingIndex = hoverImage.transform.GetSiblingIndex();
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
            _defaultImage.transform.SetSiblingIndex(_defaultImageSiblingIndex);
            _hoverImage.transform.SetSiblingIndex(_hoverImageSiblingIndex);
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

            Image currentImage = showHover ? _defaultImage : _hoverImage;
            Image nextImage = showHover ? _hoverImage : _defaultImage;
            float currentAlpha = showHover ? _defaultImageAlpha : _hoverImageAlpha;
            float nextAlpha = showHover ? _hoverImageAlpha : _defaultImageAlpha;

            currentImage.gameObject.SetActive(true);
            nextImage.gameObject.SetActive(true);
            MoveAbove(currentImage, nextImage);
            SetAlpha(currentImage, currentAlpha);
            SetAlpha(nextImage, nextAlpha);

            _imageTween = DOTween.Sequence()
                .Append(currentImage.DOFade(0f, _duration))
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (_isHovered == showHover)
                    {
                        currentImage.gameObject.SetActive(false);
                        nextImage.gameObject.SetActive(true);
                    }
                });
        }

        private static void MoveAbove(Image image, Image imageBelow)
        {
            int imageIndex = image.transform.GetSiblingIndex();
            int imageBelowIndex = imageBelow.transform.GetSiblingIndex();

            if (imageIndex < imageBelowIndex)
                image.transform.SetSiblingIndex(imageBelowIndex);
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
