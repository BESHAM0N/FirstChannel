using System;

namespace FirstChannel
{
    [Flags]
    public enum CardTarget
    {
        None = 0,
        First = 1,
        Second = 2,
        Both = First | Second
    }
}
