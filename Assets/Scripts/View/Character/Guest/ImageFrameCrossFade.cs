using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FirstChannel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class ImageFrameCrossFade : MonoBehaviour
    {
        private static readonly int IsTalkHash = Animator.StringToHash("IsTalk");
        private static readonly int IsCryHash = Animator.StringToHash("IsCry");

        [SerializeField, Min(0f)] private float _fadeDuration = 0.2f;

        private Animator _animator;
        private Image _currentImage;
        private Image _previousImage;
        private Sprite _currentSprite;
        private Tween _fadeTween;
        private bool _hasTalkParameter;
        private bool _hasCryParameter;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _currentImage = GetComponent<Image>();
            _currentSprite = _currentImage.sprite;
            _previousImage = CreatePreviousFrameImage();
            CacheAnimationParameters();
        }

        private void OnEnable()
        {
            if (_currentImage == null)
                return;

            _currentSprite = _currentImage.sprite;
            HidePreviousFrame();
        }

        private void LateUpdate()
        {
            Sprite nextSprite = _currentImage.sprite;

            if (!IsReactionAnimationPlaying())
            {
                if (_fadeTween != null || _previousImage.enabled)
                    HidePreviousFrame();

                _currentSprite = nextSprite;
                return;
            }

            if (nextSprite == _currentSprite)
                return;

            if (_currentSprite != null && _fadeDuration > 0f)
                FadePreviousFrame(_currentSprite);
            else
                HidePreviousFrame();

            _currentSprite = nextSprite;
        }

        private void CacheAnimationParameters()
        {
            if (_animator == null)
                return;

            AnimatorControllerParameter[] parameters = _animator.parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];

                if (parameter.type != AnimatorControllerParameterType.Bool)
                    continue;

                if (parameter.nameHash == IsTalkHash)
                    _hasTalkParameter = true;
                else if (parameter.nameHash == IsCryHash)
                    _hasCryParameter = true;
            }
        }

        private bool IsReactionAnimationPlaying()
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
                return false;

            return (_hasTalkParameter && _animator.GetBool(IsTalkHash)) ||
                   (_hasCryParameter && _animator.GetBool(IsCryHash));
        }

        private Image CreatePreviousFrameImage()
        {
            var overlayObject = new GameObject(
                "Previous Frame",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            overlayObject.layer = gameObject.layer;

            RectTransform overlayTransform = overlayObject.GetComponent<RectTransform>();
            overlayTransform.SetParent(transform, false);
            overlayTransform.anchorMin = Vector2.zero;
            overlayTransform.anchorMax = Vector2.one;
            overlayTransform.offsetMin = Vector2.zero;
            overlayTransform.offsetMax = Vector2.zero;
            overlayTransform.SetAsLastSibling();

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.raycastTarget = false;
            overlayImage.maskable = _currentImage.maskable;
            overlayImage.material = _currentImage.material;
            overlayImage.type = _currentImage.type;
            overlayImage.preserveAspect = _currentImage.preserveAspect;
            overlayImage.fillCenter = _currentImage.fillCenter;
            overlayImage.fillMethod = _currentImage.fillMethod;
            overlayImage.fillAmount = _currentImage.fillAmount;
            overlayImage.fillClockwise = _currentImage.fillClockwise;
            overlayImage.fillOrigin = _currentImage.fillOrigin;
            overlayImage.useSpriteMesh = _currentImage.useSpriteMesh;
            overlayImage.pixelsPerUnitMultiplier = _currentImage.pixelsPerUnitMultiplier;
            overlayImage.enabled = false;
            return overlayImage;
        }

        private void FadePreviousFrame(Sprite sprite)
        {
            KillFade();

            _previousImage.sprite = sprite;
            _previousImage.color = _currentImage.color;
            _previousImage.enabled = true;

            _fadeTween = _previousImage
                .DOFade(0f, _fadeDuration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _fadeTween = null;

                    if (_previousImage != null)
                        _previousImage.enabled = false;
                });
        }

        private void HidePreviousFrame()
        {
            KillFade();

            if (_previousImage != null)
                _previousImage.enabled = false;
        }

        private void KillFade()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
        }

        private void OnDisable()
        {
            HidePreviousFrame();
        }

        private void OnDestroy()
        {
            KillFade();
        }
    }
}
