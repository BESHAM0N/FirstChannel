using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FirstChannel
{
    public sealed class ViewersCountView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private GameObject _newCount;
        [SerializeField] private TMP_Text _newCountText;
        [SerializeField, Min(0f)] private float _totalAnimationDuration = 0.3f;
        [SerializeField, Min(0f)] private float _flyDistance = 55f;
        [SerializeField, Min(0f)] private float _flyDuration = 0.75f;

        private readonly Queue<int> _pendingValues = new Queue<int>();

        private RectTransform _newCountTransform;
        private Vector2 _newCountStartPosition;
        private Tween _totalTween;
        private Sequence _showValueSequence;
        private float _displayedTotal;

        public void Initialize(int total)
        {
            KillTweens();
            _pendingValues.Clear();
            _newCountTransform = (RectTransform)_newCount.transform;
            _newCountStartPosition = _newCountTransform.anchoredPosition;
            _displayedTotal = total;
            _text.text = total.ToString();
            _newCount.SetActive(false);
        }

        public void UpdateTotal(int total)
        {
            _totalTween?.Kill();
            _totalTween = DOVirtual.Float(
                    _displayedTotal,
                    total,
                    _totalAnimationDuration,
                    value =>
                    {
                        _displayedTotal = value;
                        _text.text = Mathf.RoundToInt(value).ToString();
                    })
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        public void ShowAddedViewers(int value)
        {
            if (value == 0)
                return;

            _pendingValues.Enqueue(value);

            if (_showValueSequence == null || !_showValueSequence.IsActive())
                PlayNextValue();
        }

        public void StopAnimations(int total)
        {
            KillTweens();
            _pendingValues.Clear();
            _displayedTotal = total;
            _text.text = total.ToString();
            _newCount.SetActive(false);
        }

        private void PlayNextValue()
        {
            if (_pendingValues.Count == 0)
            {
                _newCount.SetActive(false);
                _showValueSequence = null;
                return;
            }

            int value = _pendingValues.Dequeue();
            _newCount.SetActive(true);
            _newCountText.text = value > 0 ? $"+{value}" : value.ToString();
            _newCountText.alpha = 0f;
            _newCountTransform.anchoredPosition = _newCountStartPosition;
            _newCountTransform.localScale = Vector3.one * 0.85f;

            _showValueSequence = DOTween.Sequence()
                .Append(_newCountText.DOFade(1f, 0.12f))
                .Join(_newCountTransform.DOScale(1f, 0.18f).SetEase(Ease.OutBack))
                .AppendInterval(0.12f)
                .Append(_newCountTransform
                    .DOAnchorPosY(_newCountStartPosition.y + _flyDistance, _flyDuration)
                    .SetEase(Ease.OutCubic))
                .Join(_newCountText.DOFade(0f, _flyDuration * 0.65f).SetDelay(_flyDuration * 0.35f))
                .SetUpdate(true)
                .OnComplete(PlayNextValue);
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _totalTween?.Kill();
            _showValueSequence?.Kill();
            _totalTween = null;
            _showValueSequence = null;
        }
    }
}
