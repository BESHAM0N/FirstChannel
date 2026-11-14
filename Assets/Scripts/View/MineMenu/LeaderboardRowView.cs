using TMPro;
using UnityEngine;

namespace FirstChannel
{
    public sealed class LeaderboardRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _placeText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _valueText;

        public void SetData(LeaderboardEntry entry)
        {
            _placeText.SetText("{0}", entry.Position);
            _nameText.SetText(string.IsNullOrEmpty(entry.DisplayName) ? "Игрок" : entry.DisplayName);
            _valueText.SetText("{0}", entry.StatValue);
        }
    }
}
