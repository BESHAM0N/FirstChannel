using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FirstChannel
{
    public sealed class SoundWorld : MonoBehaviour
    {
        private AudioSource _music;
        private AudioSource _sfx;
        private AudioSource _voice;

        private readonly Dictionary<SoundType, AudioClip> _map = new Dictionary<SoundType, AudioClip>();

        private Coroutine _voiceRoutine;
        private Action _voiceCompleted;

        [Inject]
        public void Initialize(SoundConfig config)
        {
            EnsureSources();
            _map.Clear();

            if (config == null || config.Config == null)
                return;

            foreach (Sound sound in config.Config)
            {
                if (sound.AudioClip != null && !_map.ContainsKey(sound.SoundType))
                    _map.Add(sound.SoundType, sound.AudioClip);
            }
        }

        public void SetMasterMute(bool mute)
        {
            EnsureSources();
            _music.mute = mute;
            _sfx.mute = mute;
            _voice.mute = mute;
        }

        public void PlaySfx(SoundType type, float volume = 1f, float pitch = 1f)
        {
            EnsureSources();

            if (!_map.TryGetValue(type, out AudioClip clip) || clip == null)
                return;

            _sfx.pitch = pitch;
            _sfx.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void PlayMusic(SoundType type, float volume = 1f)
        {
            EnsureSources();

            if (!_map.TryGetValue(type, out AudioClip clip) || clip == null)
                return;

            if (_music.clip == clip && _music.isPlaying)
            {
                _music.volume = Mathf.Clamp01(volume);
                return;
            }

            _music.clip = clip;
            _music.volume = Mathf.Clamp01(volume);
            _music.loop = true;
            _music.Play();
        }

        public void StopMusic()
        {
            EnsureSources();
            _music.Stop();
            _music.clip = null;
        }

        public void PlayVoice(AudioClip clip, float volume, Action completed)
        {
            StopVoice();

            if (clip == null)
            {
                completed?.Invoke();
                return;
            }

            EnsureSources();
            _voice.clip = clip;
            _voice.volume = Mathf.Clamp01(volume);
            _voice.loop = false;
            _voiceCompleted = completed;
            _voice.Play();
            _voiceRoutine = StartCoroutine(WaitForVoice());
        }

        public void StopVoice()
        {
            if (_voiceRoutine != null)
            {
                StopCoroutine(_voiceRoutine);
                _voiceRoutine = null;
            }

            if (_voice != null)
            {
                _voice.Stop();
                _voice.clip = null;
            }

            Action completed = _voiceCompleted;
            _voiceCompleted = null;
            completed?.Invoke();
        }

        private void Awake()
        {
            EnsureSources();
        }

        private IEnumerator WaitForVoice()
        {
            while (_voice != null && _voice.isPlaying)
                yield return null;

            _voiceRoutine = null;
            _voice.clip = null;

            Action completed = _voiceCompleted;
            _voiceCompleted = null;
            completed?.Invoke();
        }

        private void EnsureSources()
        {
            if (_music == null)
                _music = CreateSource("Music");

            if (_sfx == null)
                _sfx = CreateSource("Sfx");

            if (_voice == null)
                _voice = CreateSource("Voice");
        }

        private AudioSource CreateSource(string sourceName)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
