using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    public sealed class SettingPanelView : MonoBehaviour
    {
        public event Action OnBackButtonClicked;
        public event Action<float> MasterVolumeChanged;
        public event Action<float> MusicVolumeChanged;
        public event Action<float> EffectVolumeChanged;

        [SerializeField] private Button _backButton;

        [Header("Volume")]
        [SerializeField] private Slider _volumeMasterSlider;
        [SerializeField] private Slider _volumeMusicSlider;
        [SerializeField] private Slider _volumeEffectSlider;
        
        [SerializeField] private UIPanelTweenView _settingPanelTween;
        [SerializeField] private UIPanelTweenView _settingBackPanelTween;

        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (_backButton != null)
                _backButton.onClick.AddListener(BackButtonClick);

            if (_volumeMasterSlider != null)
                _volumeMasterSlider.onValueChanged.AddListener(OnChangeMasterValue);

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.onValueChanged.AddListener(OnChangeMusicValue);

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.onValueChanged.AddListener(OnChangeEffectValue);
        }

        public void SetVolumeValues(float master, float music, float effects)
        {
            if (_volumeMasterSlider != null)
                _volumeMasterSlider.SetValueWithoutNotify(Mathf.Clamp01(master));

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.SetValueWithoutNotify(Mathf.Clamp01(music));

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.SetValueWithoutNotify(Mathf.Clamp01(effects));
        }

        public void OnShow()
        {
            if (_settingPanelTween != null)
                _settingPanelTween.Show();
            else
                gameObject.SetActive(true);

            if (_settingBackPanelTween != null)
                _settingBackPanelTween.Show();
        }

        public void OnHide()
        {
            if (_settingPanelTween != null)
                _settingPanelTween.Hide();
            else
                gameObject.SetActive(false);

            if (_settingBackPanelTween != null)
                _settingBackPanelTween.Hide();
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            if (_backButton != null)
                _backButton.onClick.RemoveListener(BackButtonClick);

            if (_volumeMasterSlider != null)
                _volumeMasterSlider.onValueChanged.RemoveListener(OnChangeMasterValue);

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.onValueChanged.RemoveListener(OnChangeMusicValue);

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.onValueChanged.RemoveListener(OnChangeEffectValue);

            _isInitialized = false;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnChangeMasterValue(float value)
        {
            MasterVolumeChanged?.Invoke(value);
        }

        private void OnChangeMusicValue(float value)
        {
            MusicVolumeChanged?.Invoke(value);
        }

        private void OnChangeEffectValue(float value)
        {
            EffectVolumeChanged?.Invoke(value);
        }

        private void BackButtonClick()
        {
            OnBackButtonClicked?.Invoke();
        }
    }
}
