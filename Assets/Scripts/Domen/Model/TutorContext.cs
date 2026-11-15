using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstChannel
{
    [CreateAssetMenu(fileName = "TutorContext", menuName = "Configs/Tutor")]
    public sealed class TutorContext : ScriptableObject
    {
        public List<TutorStep> TutorSteps;
    }

    [Serializable]
    public sealed class TutorStep
    {
        public TutorStepType TutorStepType;
        public string Text;
        public AudioClip VoiceClip;
    }

    public enum TutorStepType
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth,
        Seventh,
        Eighth,
        Ninth,
        Tenth,
    }
}