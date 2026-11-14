using System.Collections.Generic;
using UnityEngine;

namespace FirstChannel
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Game/Sound Config")]
    public class SoundConfig : ScriptableObject
    {
        public List<Sound> Config;
    }
}