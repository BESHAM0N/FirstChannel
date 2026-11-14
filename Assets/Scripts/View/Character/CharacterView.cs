using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FirstChannel
{
    public sealed class CharacterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TMP_Text _text;

        public void Initialize(string playerName)
        {
            gameObject.SetActive(true);
            _name.text = playerName;
            HideReplica();
        }

        public void ShowReplica(string text)
        {
            _text.text = text;
            _dialogPanel.transform.DOKill();
            _dialogPanel.SetActive(true);
            _dialogPanel.transform.localScale = Vector3.one * 0.9f;
            _dialogPanel.transform.DOScale(1f, 0.15f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void HideReplica()
        {
            _dialogPanel.transform.DOKill();
            _dialogPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            _dialogPanel.transform.DOKill();
        }
    }
}
