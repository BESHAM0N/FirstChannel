using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FirstChannel
{
    internal sealed class DialogueTypewriter
    {
        private const float CharactersPerSecond = 45f;
        private const float AvailableTimeRatio = 0.85f;
        private const float MinDuration = 0.05f;

        private readonly TMP_Text _text;
        private Tween _animation;

        public DialogueTypewriter(TMP_Text text)
        {
            _text = text;
        }

        public void Play(string value, float availableDuration)
        {
            Stop();

            _text.text = value ?? string.Empty;
            _text.maxVisibleCharacters = 0;
            _text.ForceMeshUpdate();

            int characterCount = _text.textInfo.characterCount;

            if (characterCount == 0)
                return;

            float naturalDuration = characterCount / CharactersPerSecond;
            float maxDuration = Mathf.Max(MinDuration, availableDuration * AvailableTimeRatio);
            float duration = Mathf.Min(naturalDuration, maxDuration);

            _animation = DOTween.To(
                    () => _text.maxVisibleCharacters,
                    visibleCharacters => _text.maxVisibleCharacters = visibleCharacters,
                    characterCount,
                    duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() => _animation = null);
        }

        public void Clear()
        {
            Stop();
            _text.text = string.Empty;
            _text.maxVisibleCharacters = int.MaxValue;
        }

        public void Stop()
        {
            _animation?.Kill();
            _animation = null;
        }
    }
}
