using System;

namespace FirstChannel
{
    public sealed class EmotionBar
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;
        public const int FirstFireThreshold = 70;
        public const int SecondFireThreshold = 30;

        public event Action<int> ValueChanged;
        public event Action Depleted;

        public int CurrentValue { get; private set; } = MaxValue;

        public int Apply(int value)
        {
            int previousValue = CurrentValue;
            CurrentValue = Math.Max(MinValue, Math.Min(MaxValue, CurrentValue + value));

            if (CurrentValue == previousValue)
                return CurrentValue;

            ValueChanged?.Invoke(CurrentValue);

            if (CurrentValue == MinValue)
                Depleted?.Invoke();

            return CurrentValue;
        }
    }
}
