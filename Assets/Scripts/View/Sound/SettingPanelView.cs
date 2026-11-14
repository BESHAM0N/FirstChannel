using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FirstChannel
{
    public sealed class SettingPanelView : MonoBehaviour
    {  
        public event Action OnBackButtonClicked; 
        
        [SerializeField] private Button _backButton;

        [Header("Volume")]
        [SerializeField] private Slider _volumeMasterSlider;
        [SerializeField] private Slider _volumeMusicSlider;
        [SerializeField] private Slider _volumeEffectSlider;
        
        [SerializeField] private UIPanelTweenView _settingPanelTween;
        [SerializeField] private UIPanelTweenView _settingBackPanelTween;
        
        [Inject] private ISoundService _sound;
        
        public void OnShow()
        {
            UpdateSliders();
     
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
       
        private void Awake()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(BackButtonClick);

            if (_volumeMasterSlider != null)
                _volumeMasterSlider.onValueChanged.AddListener(OnChangeMasterValue);

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.onValueChanged.AddListener(OnChangeMusicValue);

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.onValueChanged.AddListener(OnChangeEffectValue);
        }
        
        private void OnDestroy()
        {
            if (_backButton != null)
                _backButton.onClick.RemoveListener(BackButtonClick);

            if (_volumeMasterSlider != null)
                _volumeMasterSlider.onValueChanged.RemoveListener(OnChangeMasterValue);

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.onValueChanged.RemoveListener(OnChangeMusicValue);

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.onValueChanged.RemoveListener(OnChangeEffectValue);
        }
        
        private void UpdateSliders()
        {
            if (_volumeMasterSlider != null)
                _volumeMasterSlider.SetValueWithoutNotify(_sound.MasterVolume);

            if (_volumeMusicSlider != null)
                _volumeMusicSlider.SetValueWithoutNotify(_sound.MusicVolume);

            if (_volumeEffectSlider != null)
                _volumeEffectSlider.SetValueWithoutNotify(_sound.EffectVolume);
        }

        private void OnChangeMasterValue(float value)
        {
            _sound.ChangeMasterVolume(value);
        }
        
        private void OnChangeMusicValue(float value)
        {
            _sound.ChangeMusicVolume(value);
        }
        
        private void OnChangeEffectValue(float value)
        {
            _sound.ChangeEffectVolume(value);
        }

        private void BackButtonClick()
        {
            OnBackButtonClicked?.Invoke();   
        }
    }
}