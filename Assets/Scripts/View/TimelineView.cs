using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class TimelineView : MonoBehaviour
    {
        public event Action<float> MasterVolumeChanged;

        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Slider _masterVolumeSlider;

        public void Initialize(float progress, string textTime, float masterVolume)
        {
            ColorBlock colors = _slider.colors;
            colors.disabledColor = colors.normalColor;
            _slider.colors = colors;
            _slider.interactable = false;

            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(masterVolume));
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            UpdateTimeline(progress, textTime);
        }

        public void UpdateTimeline(float progress, string textTime)
        {
            _slider.SetValueWithoutNotify(Mathf.Clamp01(progress));
            _text.text = textTime;
        }

        public void SetMasterVolume(float value)
        {
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(value));
        }

        public void Dispose()
        {
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        private void OnMasterVolumeChanged(float value)
        {
            MasterVolumeChanged?.Invoke(value);
        }
    }
}
