using System.Collections.Generic;
using UnityEngine;

namespace FirstChannel
{
    public sealed class HandCardView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _cardSpacing = 220f;
        [SerializeField, Min(0f)] private float _arcHeight = 28f;
        [SerializeField, Range(0f, 20f)] private float _fanAngle = 8f;
        [SerializeField, Min(0f)] private float _layoutDuration = 0.3f;
        [SerializeField, Min(0f)] private float _dealDelay = 0.65f;

        private RectTransform RectTransform => (RectTransform)transform;

        public void LayoutCards(IReadOnlyList<CardView> cards, bool isInitialDeal)
        {
            int count = cards.Count;

            for (int i = 0; i < count; i++)
            {
                float offset = i - (count - 1) * 0.5f;
                float normalized = count <= 1 ? 0f : offset / ((count - 1) * 0.5f);
                float height = _arcHeight * (1f - Mathf.Abs(normalized));
                var position = new Vector2(offset * _cardSpacing, height);
                float rotation = -normalized * _fanAngle;
                float delay = isInitialDeal ? i * _dealDelay : 0f;

                cards[i].SetHomePose(
                    RectTransform,
                    position,
                    rotation,
                    i,
                    _layoutDuration,
                    delay);
            }
        }
    }
}
