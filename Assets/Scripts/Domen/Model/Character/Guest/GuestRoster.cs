using System.Collections.Generic;

namespace FirstChannel
{
    public sealed class GuestRoster
    {
        private readonly Dictionary<GuestId, Guest> _guests = new Dictionary<GuestId, Guest>();

        public GuestRoster(ReplicaListConfig[] configs)
        {
            if (configs == null)
                return;

            for (int i = 0; i < configs.Length; i++)
            {
                ReplicaListConfig config = configs[i];

                if (config != null && !_guests.ContainsKey(config.GuestId))
                    _guests.Add(config.GuestId, new Guest(config));
            }
        }

        public bool TryGetGuest(GuestId guestId, out Guest guest)
        {
            return _guests.TryGetValue(guestId, out guest);
        }

        public int GetViewersMultiplier()
        {
            int multiplier = 1;

            foreach (Guest guest in _guests.Values)
                multiplier *= guest.EmotionBar.ViewersMultiplier;

            return multiplier;
        }
    }
}
