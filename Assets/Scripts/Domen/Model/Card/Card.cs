using UnityEngine;

namespace FirstChannel
{
    public sealed class Card
    {
        private string _nameCard;
        private string _descriptionCard;
        private int _addViewers;
        private int _addEmotionCharacter;
        private CardType _cardType;
        private CardId _cardId;
        private CardTarget _allowedTargets;
        private Sprite _imageCard;
        
        public Sprite ImageCard => _imageCard;
        public string NameCard => _nameCard;
        public string DescriptionCard => _descriptionCard;
        public int AddEmotionCharacter => _addEmotionCharacter;
        public int AddViewers => _addViewers;
        public CardType CardType => _cardType;
        public CardId CardId => _cardId;
        public CardTarget AllowedTargets => _allowedTargets;
        
        public void Initialize(CardConfig config)
        {
            _nameCard = config.Name;
            _descriptionCard = config.Description;
            _addViewers = config.AddViewers;
            _addEmotionCharacter = config.AddEmotionCharacter;
            _cardType = config.CardType;
            _cardId = config.CardId;
            _allowedTargets = config.AllowedTargets;
            _imageCard = config.Image;
        }

        public bool CanTarget(GuestId guestId)
        {
            CardTarget target = guestId == GuestId.First
                ? CardTarget.First
                : CardTarget.Second;

            return (_allowedTargets & target) != 0;
        }
    }
}
