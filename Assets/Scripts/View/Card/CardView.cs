using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace FirstChannel
{
    public sealed class CardView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public event Action<CardView, GuestView> DropRequested;

        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _emotionValue;
        [SerializeField] private TMP_Text _viewersValue;
        [Header("Раздача из колоды")]
        [SerializeField] private Sprite _backSprite;
        [SerializeField] private Color _backColor = new Color(0.16f, 0.11f, 0.3f, 1f);
        [SerializeField, Min(0.1f)] private float _dealDuration = 0.6f;
        [SerializeField, Min(0.1f)] private float _flipDuration = 0.32f;
        [SerializeField, Min(0f)] private float _dealJumpPower = 110f;
        [SerializeField, Range(0.1f, 1f)] private float _dealStartScale = 0.78f;

        private const float HoverOffset = 70f;
        private const float HoverScale = 1.08f;
        private const float HoverDuration = 0.18f;
        private const float ReturnDuration = 0.25f;
        private const float ApplyDuration = 0.28f;

        private RectTransform _rectTransform;
        private RectTransform _homeParent;
        private RectTransform _dragLayer;
        private CanvasGroup _canvasGroup;
        private Sequence _motion;
        private Vector2 _homePosition;
        private float _homeRotation;
        private int _homeSiblingIndex;
        private bool _isDragging;
        private bool _isConsumed;
        private bool _isDealing;
        private bool _isInteractionEnabled = true;
        private Sprite _frontSprite;
        private Color _frontColor;

        public void SetCard(
            Sprite sprite,
            string title,
            string description,
            string emotionValue,
            string viewersValue)
        {
            _frontSprite = sprite;
            _image.sprite = _frontSprite;
            _image.color = _frontColor;
            _title.text = title;
            _description.text = description;
            _emotionValue.text = emotionValue;
            _viewersValue.text = viewersValue;
        }

        public void SetEffectValues(int emotionValue, int viewersValue)
        {
            _emotionValue.text = emotionValue.ToString();
            _viewersValue.text = viewersValue.ToString();
        }

        public void SetHomePose(
            RectTransform parent,
            Vector2 anchoredPosition,
            float rotation,
            int siblingIndex,
            float duration,
            float delay = 0f)
        {
            _homeParent = parent;
            _homePosition = anchoredPosition;
            _homeRotation = rotation;
            _homeSiblingIndex = siblingIndex;

            if (_isDragging || _isConsumed)
                return;

            if (_rectTransform.parent != parent)
                _rectTransform.SetParent(parent, true);

            _rectTransform.SetSiblingIndex(siblingIndex);

            if (_isDealing)
            {
                AnimateDealToHome(delay);
                return;
            }

            AnimateToHome(duration, delay);
        }

        public void SetDealOrigin(Vector3 worldPosition)
        {
            KillMotion();
            _isDealing = true;
            _rectTransform.position = worldPosition;
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.localScale = Vector3.one * _dealStartScale;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            ShowBack();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            if (_isConsumed)
                return;

            _isInteractionEnabled = enabled;
            _canvasGroup.interactable = enabled && !_isDealing;
            _canvasGroup.blocksRaycasts = enabled && !_isDealing;

            if (!enabled && !_isDragging && !_isDealing && _homeParent != null)
            {
                _rectTransform.SetSiblingIndex(_homeSiblingIndex);
                AnimateToHome(HoverDuration);
            }
        }

        public void ReturnToHand()
        {
            if (_homeParent == null || _isConsumed)
                return;

            _canvasGroup.blocksRaycasts = true;
            _rectTransform.SetParent(_homeParent, true);
            _rectTransform.SetSiblingIndex(_homeSiblingIndex);
            AnimateToHome(ReturnDuration);
        }

        public void PlayApplied(GuestView target, Action onComplete)
        {
            _isConsumed = true;
            _canvasGroup.blocksRaycasts = false;
            KillMotion();

            _motion = DOTween.Sequence()
                .Join(_rectTransform.DOMove(target.transform.position, ApplyDuration).SetEase(Ease.InCubic))
                .Join(_rectTransform.DOScale(0.15f, ApplyDuration).SetEase(Ease.InBack))
                .Join(_rectTransform.DORotate(Vector3.zero, ApplyDuration))
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractionEnabled || _isDragging || _isConsumed || _homeParent == null)
                return;

            KillMotion();
            _rectTransform.SetAsLastSibling();
            _motion = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPos(_homePosition + Vector2.up * HoverOffset, HoverDuration))
                .Join(_rectTransform.DOLocalRotate(Vector3.zero, HoverDuration))
                .Join(_rectTransform.DOScale(HoverScale, HoverDuration))
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractionEnabled || _isDragging || _isConsumed)
                return;

            _rectTransform.SetSiblingIndex(_homeSiblingIndex);
            AnimateToHome(HoverDuration);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isInteractionEnabled || _isConsumed)
                return;

            _isDragging = true;
            _canvasGroup.blocksRaycasts = false;
            KillMotion();

            Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;
            _dragLayer = (RectTransform)canvas.transform;
            _rectTransform.SetParent(_dragLayer, true);
            _rectTransform.SetAsLastSibling();
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isInteractionEnabled || !_isDragging || _dragLayer == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dragLayer,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;

            GuestView target = eventData.pointerCurrentRaycast.gameObject != null
                ? eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<GuestView>()
                : null;

            if (DropRequested == null)
            {
                ReturnToHand();
                return;
            }

            DropRequested.Invoke(this, target);
        }

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            _frontColor = _image.color;

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            KillMotion();
        }

        private void AnimateToHome(float duration, float delay = 0f)
        {
            KillMotion();
            _motion = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPos(_homePosition, duration))
                .Join(_rectTransform.DOLocalRotate(new Vector3(0f, 0f, _homeRotation), duration))
                .Join(_rectTransform.DOScale(1f, duration))
                .SetDelay(delay)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        private void AnimateDealToHome(float delay)
        {
            KillMotion();

            Vector3 targetWorldPosition = _homeParent.TransformPoint(_homePosition);
            float halfFlipDuration = _flipDuration * 0.5f;
            float flipDelay = Mathf.Max(0f, (_dealDuration - _flipDuration) * 0.45f);

            Sequence flip = DOTween.Sequence()
                .AppendInterval(flipDelay)
                .Append(_rectTransform
                    .DOLocalRotate(new Vector3(0f, 90f, 0f), halfFlipDuration)
                    .SetEase(Ease.InQuad))
                .AppendCallback(ShowFront)
                .Append(_rectTransform
                    .DOLocalRotate(new Vector3(0f, 0f, _homeRotation), halfFlipDuration)
                    .SetEase(Ease.OutQuad));

            _motion = DOTween.Sequence()
                .SetDelay(delay)
                .Join(_rectTransform
                    .DOJump(targetWorldPosition, _dealJumpPower, 1, _dealDuration)
                    .SetEase(Ease.OutCubic))
                .Join(_rectTransform.DOScale(1f, _dealDuration).SetEase(Ease.OutCubic))
                .Join(flip)
                .SetUpdate(true)
                .OnComplete(CompleteDeal);
        }

        private void CompleteDeal()
        {
            _motion = null;
            _isDealing = false;
            ShowFront();
            _rectTransform.anchoredPosition = _homePosition;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _homeRotation);
            _rectTransform.localScale = Vector3.one;
            _rectTransform.SetSiblingIndex(_homeSiblingIndex);
            _canvasGroup.interactable = _isInteractionEnabled;
            _canvasGroup.blocksRaycasts = _isInteractionEnabled;
        }

        private void ShowBack()
        {
            _image.sprite = _backSprite;
            _image.color = _backColor;
            SetFrontContentActive(false);
        }

        private void ShowFront()
        {
            _image.sprite = _frontSprite;
            _image.color = _frontColor;
            SetFrontContentActive(true);
        }

        private void SetFrontContentActive(bool isActive)
        {
            _title.gameObject.SetActive(isActive);
            _description.gameObject.SetActive(isActive);
            _emotionValue.gameObject.SetActive(isActive);
            _viewersValue.gameObject.SetActive(isActive);
        }

        private void KillMotion()
        {
            _motion?.Kill();
            _motion = null;
        }

        public sealed class Factory : PlaceholderFactory<CardView>
        {
        }
    }
}
