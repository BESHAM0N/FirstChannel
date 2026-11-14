using TMPro;
using UnityEngine;

using DG.Tweening;

namespace FirstChannel
{
    public sealed class GuestView : MonoBehaviour
    {
        [SerializeField] private GuestId _id;
        [SerializeField] private EmotionBarView _emotionBarView;
        [SerializeField] private GameObject _firstFireAnimation;
        [SerializeField] private GameObject _secondFireAnimation;
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TMP_Text _dialogText;

        public GuestId Id => _id;

        public void InitializeEmotion(int value)
        {
            _emotionBarView.Initialize(value);
            UpdateFire(value);
        }

        public void UpdateEmotion(int value)
        {
            _emotionBarView.UpdateEmotionBar(value);
            UpdateFire(value);
        }

        public void ShowReplica(string text)
        {
            _dialogText.text = text;
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

        public void PlayCardAppliedFeedback()
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.05f, 0.25f, 6, 0.6f)
                .SetUpdate(true);
        }

        private void Awake()
        {
            HideReplica();
        }

        private void UpdateFire(int value)
        {
            bool isSecondFire = value <= EmotionBar.SecondFireThreshold;
            bool isFirstFire = value <= EmotionBar.FirstFireThreshold && !isSecondFire;

            _firstFireAnimation.SetActive(isFirstFire);
            _secondFireAnimation.SetActive(isSecondFire);
        }
    }
}
