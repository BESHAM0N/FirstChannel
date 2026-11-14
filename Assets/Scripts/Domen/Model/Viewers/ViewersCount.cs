using System;

namespace FirstChannel
{
    public sealed class ViewersCount
    {
        public event Action<int, int> Changed;

        public int Current { get; private set; }

        public void Add(int value)
        {
            if (value == 0)
                return;

            int previous = Current;
            Current = Math.Max(0, Current + value);
            int appliedValue = Current - previous;

            if (appliedValue != 0)
                Changed?.Invoke(Current, appliedValue);
        }
    }
}
