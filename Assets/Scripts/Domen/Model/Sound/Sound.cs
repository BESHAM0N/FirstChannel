using System;
using UnityEngine;

namespace FirstChannel
{
    [Serializable]
    public class Sound
    {
        public SourceType SourceType;
        public SoundType SoundType;
        public AudioClip AudioClip;
    }
}