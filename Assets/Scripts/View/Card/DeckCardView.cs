using DG.Tweening;
using UnityEngine;

namespace FirstChannel
{
    public sealed class DeckCardView : MonoBehaviour
    {
        public Vector3 CardOrigin => transform.position;

        public void PlayDrawFeedback()
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.08f, 0.2f, 5, 0.5f)
                .SetUpdate(true);
        }
    }
}
