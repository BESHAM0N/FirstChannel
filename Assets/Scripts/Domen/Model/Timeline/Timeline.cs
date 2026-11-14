using System;

namespace FirstChannel
{
    public sealed class Timeline
    {
        public const float DefaultDurationSeconds = 180f;

        public event Action Updated;
        public event Action Finished;

        public float DurationSeconds { get; }
        public float ElapsedSeconds { get; private set; }
        public float Progress => ElapsedSeconds / DurationSeconds;
        public bool IsFinished { get; private set; }
        public bool IsStopped { get; private set; }

        public Timeline(float durationSeconds)
        {
            if (durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));

            DurationSeconds = durationSeconds;
        }

        public void Advance(float deltaTime)
        {
            if (IsFinished || IsStopped || deltaTime <= 0f)
                return;

            ElapsedSeconds = Math.Min(ElapsedSeconds + deltaTime, DurationSeconds);
            IsFinished = ElapsedSeconds >= DurationSeconds;

            Updated?.Invoke();

            if (IsFinished)
                Finished?.Invoke();
        }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
