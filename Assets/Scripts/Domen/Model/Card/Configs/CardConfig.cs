using UnityEngine;

namespace FirstChannel
{
    [CreateAssetMenu(fileName = "CardConfig", menuName = "Configs/Card")]
    public sealed class CardConfig : ScriptableObject
    {
        [Header("Картинка")]
        public Sprite Image;
        [Header("Название")]
        public string Name;
        [Header("Описание")]
        public string Description;
        [Header("Количество просмотров")]
        public int  AddViewers;
        [Header("Количество терпения")]
        public int AddEmotionCharacter;
        [Header("Тип карты")]
        public CardType CardType;
        [Header("Идентификатор реплики")]
        public CardId CardId;
        [Header("Допустимые цели")]
        public CardTarget AllowedTargets = CardTarget.Both;
    }
}
