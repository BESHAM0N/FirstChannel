using System;
using UnityEngine;

namespace FirstChannel
{
    [Serializable]
    public struct Replica
    {
        public CardId CardId;
        [TextArea] public string Text;
        public AudioClip VoiceClip;
        [Min(0.1f)] public float FallbackDuration;

        public float Duration => VoiceClip != null
            ? VoiceClip.length
            : Mathf.Max(0.1f, FallbackDuration);
    }
}
