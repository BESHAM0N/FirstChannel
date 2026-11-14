using System.Collections.Generic;
using UnityEngine;

namespace FirstChannel
{
    [CreateAssetMenu(fileName = "ReplicaListConfig", menuName = "Configs/Replica List")]
    public class ReplicaListConfig : ScriptableObject
    {
        public GuestId GuestId;
        [Header("Вводная реплика")]
        public Replica IntroReplica;
        [Header("Реакции на карты")]
        public List<Replica> Replicas;
    }
}
