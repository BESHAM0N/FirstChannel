namespace FirstChannel
{
    public interface ISoundService
    {
        bool IsSoundEnabled { get; }

        float MasterVolume { get; }
        float MusicVolume { get; }
        float EffectVolume { get; }

        void ToggleSound();
        void SetEnabled(bool enabled);

        void ChangeMasterVolume(float volume);
        void ChangeMusicVolume(float volume);
        void ChangeEffectVolume(float volume);

        void Play(SoundType type, float volumeMultiplier = 1f, float pitch = 1f);
        void PlayAndWait(SoundType type, System.Action completed);
        void PlayLoop(SoundType type);
        void StopLoop();
        void PlayVoice(UnityEngine.AudioClip clip, System.Action completed);
        void StopVoice();
    }
}
