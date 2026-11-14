using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class EmotionBarView : MonoBehaviour
    {
        [SerializeField] private Scrollbar _emotionBarSlider;
        [SerializeField] private Image _emotionBarImage;
        [SerializeField] private TMP_Text _emotionBarText;

        [SerializeField] private Color _colorTop;
        [SerializeField] private Color _colorMid;
        [SerializeField] private Color _colorBot;
        [SerializeField, Min(0f)] private float _animationDuration = 0.35f;

        private Tween _valueTween;
        private Tween _colorTween;
        private float _displayedValue;

        public void Initialize(int value)
        {
            KillTweens();
            _displayedValue = value;
            SetDisplayedValue(value);
            _emotionBarImage.color = GetColor(value);
        }

        public void UpdateEmotionBar(int value)
        {
            KillTweens();

            _valueTween = DOVirtual.Float(
                    _displayedValue,
                    value,
                    _animationDuration,
                    SetDisplayedValue)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            _colorTween = _emotionBarImage
                .DOColor(GetColor(value), _animationDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        private void SetDisplayedValue(float value)
        {
            _displayedValue = value;
            _emotionBarSlider.value = value / EmotionBar.MaxValue;
            _emotionBarText.text = Mathf.RoundToInt(value).ToString();
        }

        private Color GetColor(float value)
        {
            if (value <= EmotionBar.SecondFireThreshold)
                return _colorBot;

            if (value <= EmotionBar.FirstFireThreshold)
                return _colorMid;

            return _colorTop;
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _valueTween?.Kill();
            _colorTween?.Kill();
            _valueTween = null;
            _colorTween = null;
        }
    }
}
