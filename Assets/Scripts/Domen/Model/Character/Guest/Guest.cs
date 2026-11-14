using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class Guest
    {
        private readonly Dictionary<CardId, Replica> _replicas = new Dictionary<CardId, Replica>();

        public GuestId Id { get; }
        public EmotionBar EmotionBar { get; } = new EmotionBar();

        public Guest(ReplicaListConfig config)
        {
            Id = config.GuestId;

            if (config.Replicas == null)
                return;

            for (int i = 0; i < config.Replicas.Count; i++)
            {
                Replica replica = config.Replicas[i];

                if (!_replicas.ContainsKey(replica.CardId))
                    _replicas.Add(replica.CardId, replica);
            }
        }

        public bool TryGetReplica(CardId cardId, out Replica replica)
        {
            return _replicas.TryGetValue(cardId, out replica);
        }

        public int ApplyEmotion(int value)
        {
            return EmotionBar.Apply(value);
        }
    }
}
