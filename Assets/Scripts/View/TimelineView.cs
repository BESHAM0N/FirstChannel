using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class TimelineView : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _text;

        public void Initialize(float progress, string textTime)
        {
            ColorBlock colors = _slider.colors;
            colors.disabledColor = colors.normalColor;
            _slider.colors = colors;
            _slider.interactable = false;

            UpdateTimeline(progress, textTime);
        }

        public void UpdateTimeline(float progress, string textTime)
        {
            _slider.SetValueWithoutNotify(Mathf.Clamp01(progress));
            _text.text = textTime;
        }
    }
}
