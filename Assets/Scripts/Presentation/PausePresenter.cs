using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace FirstChannel
{
    public sealed class PausePresenter : IInitializable, IDisposable
    {
        private readonly PauseView _pauseView;
        private readonly SettingPanelView _settingPanelView;
        private readonly ISoundService _sound;
        private readonly string _mainMenuSceneName;

        private float _timeScaleBeforePause = 1f;
        private bool _isPaused;

        public PausePresenter(
            PauseView pauseView,
            SettingPanelView settingPanelView,
            ISoundService sound,
            string mainMenuSceneName)
        {
            _pauseView = pauseView;
            _settingPanelView = settingPanelView;
            _sound = sound;
            _mainMenuSceneName = mainMenuSceneName;
        }

        public void Initialize()
        {
            _pauseView.Initialize();
            _settingPanelView.Initialize();

            _pauseView.OpenRequested += Open;
            _pauseView.CloseRequested += Close;
            _pauseView.MainMenuRequested += OpenMainMenu;
            _settingPanelView.OnBackButtonClicked += Close;
            _settingPanelView.MasterVolumeChanged += _sound.ChangeMasterVolume;
            _settingPanelView.MusicVolumeChanged += _sound.ChangeMusicVolume;
            _settingPanelView.EffectVolumeChanged += _sound.ChangeEffectVolume;

            SyncSliders();
            _pauseView.Hide();
        }

        public void Dispose()
        {
            _pauseView.OpenRequested -= Open;
            _pauseView.CloseRequested -= Close;
            _pauseView.MainMenuRequested -= OpenMainMenu;
            _settingPanelView.OnBackButtonClicked -= Close;
            _settingPanelView.MasterVolumeChanged -= _sound.ChangeMasterVolume;
            _settingPanelView.MusicVolumeChanged -= _sound.ChangeMusicVolume;
            _settingPanelView.EffectVolumeChanged -= _sound.ChangeEffectVolume;

            ResumeGame();
            _settingPanelView.Dispose();
            _pauseView.Dispose();
        }

        private void Open()
        {
            if (_isPaused)
                return;

            _sound.Play(SoundType.ButtonClick);
            _isPaused = true;
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            _sound.PauseVoice();

            SyncSliders();
            _pauseView.Show();
            _settingPanelView.OnShow();
        }

        private void Close()
        {
            if (!_isPaused)
                return;

            _sound.Play(SoundType.ButtonClick);
            _settingPanelView.OnHide();
            _pauseView.Hide();
            ResumeGame();
        }

        private void OpenMainMenu()
        {
            _sound.Play(SoundType.ButtonClick);
            ResumeGame();
            _sound.StopLoop();
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void SyncSliders()
        {
            _settingPanelView.SetVolumeValues(
                _sound.MasterVolume,
                _sound.MusicVolume,
                _sound.EffectVolume);
        }

        private void ResumeGame()
        {
            if (!_isPaused)
                return;

            Time.timeScale = _timeScaleBeforePause;
            _sound.ResumeVoice();
            _isPaused = false;
        }
    }
}
