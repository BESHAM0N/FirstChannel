using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using System;

namespace FirstChannel
{
      public sealed class SoundService : ISoundService, IDisposable
    {
        private const string SOUND_ENABLED_PREF_KEY = "sound_enabled";

        private const string MASTER_VOLUME_PREF_KEY = "master_volume";
        private const string MUSIC_VOLUME_PREF_KEY = "music_volume";
        private const string EFFECT_VOLUME_PREF_KEY = "effect_volume";

        private const float DEFAULT_MASTER_VOLUME = 0.4f;
        private const float DEFAULT_MUSIC_VOLUME = 0.4f;
        private const float DEFAULT_EFFECT_VOLUME = 0.4f;

        private const float MUSIC_VOLUME_MULTIPLIER = 1f;
        private const float SFX_VOLUME_MULTIPLIER = 1f;

        public bool IsSoundEnabled { get; private set; } = true;

        public float MasterVolume { get; private set; } = DEFAULT_MASTER_VOLUME;
        public float MusicVolume { get; private set; } = DEFAULT_MUSIC_VOLUME;
        public float EffectVolume { get; private set; } = DEFAULT_EFFECT_VOLUME;

        private readonly SoundWorld _player;
        private readonly HashSet<SoundType> _bgmTypes;

        private SoundType? _lastMusic;

        [Inject]
        public SoundService(SoundWorld player)
        {
            _player = player;

            _bgmTypes = new HashSet<SoundType>
            {
                SoundType.MainMenuBackgroundMusic,
                SoundType.GameBackgroundMusic,
                SoundType.GameOverBackgroundMusic
            };

            LoadSettings();
            Apply();

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void ToggleSound()
        {
            SetEnabled(!IsSoundEnabled);
        }

        public void SetEnabled(bool enabled)
        {
            if (IsSoundEnabled == enabled)
                return;

            IsSoundEnabled = enabled;

            PlayerPrefs.SetInt(SOUND_ENABLED_PREF_KEY, enabled ? 1 : 0);
            PlayerPrefs.Save();

            Apply();

            RestartMusicIfNeeded();
        }

        public void ChangeMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);

            PlayerPrefs.SetFloat(MASTER_VOLUME_PREF_KEY, MasterVolume);
            PlayerPrefs.Save();

            Apply();

            RestartMusicIfNeeded();
        }

        public void ChangeMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);

            PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF_KEY, MusicVolume);
            PlayerPrefs.Save();

            RestartMusicIfNeeded();
        }

        public void ChangeEffectVolume(float volume)
        {
            EffectVolume = Mathf.Clamp01(volume);

            PlayerPrefs.SetFloat(EFFECT_VOLUME_PREF_KEY, EffectVolume);
            PlayerPrefs.Save();
        }

        public void Play(SoundType type, float volumeMultiplier = 1f, float pitch = 1f)
        {
            if (_bgmTypes.Contains(type))
            {
                _lastMusic = type;

                if (!IsSoundEnabled)
                    return;

                _player.PlayMusic(type, GetMusicVolume(volumeMultiplier));
                return;
            }

            if (!IsSoundEnabled)
                return;

            _player.PlaySfx(type, GetSfxVolume(volumeMultiplier), pitch);
        }

        public void PlayAndWait(SoundType type, Action completed)
        {
            if (!IsSoundEnabled)
            {
                completed?.Invoke();
                return;
            }

            _player.PlaySfx(type, GetSfxVolume(1f), 1f, completed);
        }

        public void PlayLoop(SoundType type)
        {
            _lastMusic = type;

            if (!IsSoundEnabled)
                return;

            _player.PlayMusic(type, GetMusicVolume(1f));
        }

        public void StopLoop()
        {
            _player.StopMusic();
        }

        public void PlayVoice(AudioClip clip, Action completed)
        {
            _player.PlayVoice(clip, GetSfxVolume(1f), completed);
        }

        public void StopVoice()
        {
            _player.StopVoice();
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _player.StopVoice();
        }

        private float GetMusicVolume(float volumeMultiplier)
        {
            return Mathf.Clamp01(
                volumeMultiplier *
                MasterVolume *
                MusicVolume *
                MUSIC_VOLUME_MULTIPLIER);
        }

        private float GetSfxVolume(float volumeMultiplier)
        {
            return Mathf.Clamp01(
                volumeMultiplier *
                MasterVolume *
                EffectVolume *
                SFX_VOLUME_MULTIPLIER);
        }

        private void LoadSettings()
        {
            IsSoundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_PREF_KEY, 1) == 1;

            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_PREF_KEY, DEFAULT_MASTER_VOLUME);
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF_KEY, DEFAULT_MUSIC_VOLUME);
            EffectVolume = PlayerPrefs.GetFloat(EFFECT_VOLUME_PREF_KEY, DEFAULT_EFFECT_VOLUME);

            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            EffectVolume = Mathf.Clamp01(EffectVolume);
        }
        
        private void Apply()
        {
            _player.SetMasterMute(!IsSoundEnabled);
        }

        private void RestartMusicIfNeeded()
        {
            if (!IsSoundEnabled)
                return;

            if (!_lastMusic.HasValue)
                return;

            _player.PlayMusic(_lastMusic.Value, GetMusicVolume(1f));
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Apply();
        }
    }
}
