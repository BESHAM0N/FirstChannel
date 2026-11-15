using System;

namespace FirstChannel
{
    public sealed class EmotionBar
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;
        public const int FirstFireThreshold = 40;
        public const int SecondFireThreshold = 70;
        public const int FirstFireViewersMultiplier = 2;
        public const int SecondFireViewersMultiplier = 4;

        public event Action<int> ValueChanged;
        public event Action LimitReached;

        public int CurrentValue { get; private set; } = MinValue;
        public int ViewersMultiplier => GetViewersMultiplier(CurrentValue);

        public static int GetViewersMultiplier(int value)
        {
            if (value >= SecondFireThreshold)
                return SecondFireViewersMultiplier;

            if (value >= FirstFireThreshold)
                return FirstFireViewersMultiplier;

            return 1;
        }

        public int Apply(int value)
        {
            int previousValue = CurrentValue;
            CurrentValue = Math.Max(MinValue, Math.Min(MaxValue, CurrentValue + value));

            if (CurrentValue == previousValue)
                return CurrentValue;

            ValueChanged?.Invoke(CurrentValue);

            if (CurrentValue == MaxValue)
                LimitReached?.Invoke();

            return CurrentValue;
        }
    }
}
