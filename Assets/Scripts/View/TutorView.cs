using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace FirstChannel
{
    public sealed class TutorView : MonoBehaviour
    {
        private static readonly int IsTalkHash = Animator.StringToHash("IsTalk");
        private static readonly int IsCryHash = Animator.StringToHash("IsCry");

        private const int RequiredStepCount = 10;
        private const string NextButtonText = "Далее";
        private const string StartButtonText = "Начать";

        private const float ArrowOffset = 16f;
        private const float ArrowMoveDuration = 0.65f;
        private const float PulseScale = 1.06f;
        private const float PulseDuration = 0.65f;
        private const float BarStepDuration = 1.1f;

        [SerializeField] private GameObject _tint;
        [SerializeField] private MainCharacterTutor _mainCharacter;

        [SerializeField] private GameObject[] _guests;
        [SerializeField] private GameObject _cards;
        [SerializeField] private GameObject _deckCards;
        [SerializeField] private GameObject _timeline;

        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _nextButtonText;
        [SerializeField] private TutorContext _tutorContext;
        [SerializeField] private string _gameSceneName = "GameScene";

        private readonly Dictionary<RectTransform, Vector2> _arrowPositions = new();
        private readonly Dictionary<RectTransform, Tween> _arrowTweens = new();
        private readonly Dictionary<Transform, Vector3> _pulseScales = new();
        private readonly Dictionary<Transform, Tween> _pulseTweens = new();

        private RectTransform[] _allArrows;
        private RectTransform[] _barArrows;
        private RectTransform[] _viewerArrows;
        private RectTransform[] _cardFirstArrows;
        private RectTransform[] _cardSecondArrows;
        private RectTransform[] _timelineFirstArrows;
        private RectTransform[] _timelineSecondArrows;
        private Transform[] _cardTransforms;
        private GameObject[] _guestFires;
        private GameObject _firstGuestFire;
        private GameObject _secondGuestFire;
        private EmotionBarView[] _emotionBars;
        private Sequence _barSequence;
        private int _currentStep;
        private bool _isInitialized;

        [Inject] private ISoundService _soundService;

        private void Start()
        {
            CacheSceneObjects();

            if (!ValidateConfiguration())
                return;

            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _soundService.PlayLoop(SoundType.GameOverBackgroundMusic);

            SetGuestsToIdle();
            _mainCharacter.gameObject.SetActive(true);
            _currentStep = 0;
            _isInitialized = true;
            ApplyStep(_currentStep);
        }

        private void OnDestroy()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNextButtonClicked);

            _barSequence?.Kill();

            foreach (Tween tween in _pulseTweens.Values)
                tween?.Kill();

            foreach (Tween tween in _arrowTweens.Values)
                tween?.Kill();

            _soundService?.StopVoice();
        }

        private void CacheSceneObjects()
        {
            Transform uiRoot = _tint.transform.parent;
            RectTransform[] rectTransforms = uiRoot.GetComponentsInChildren<RectTransform>(true);

            _allArrows = rectTransforms
                .Where(rect => rect.name.StartsWith("Arrow"))
                .ToArray();

            foreach (RectTransform arrow in _allArrows)
                _arrowPositions[arrow] = arrow.anchoredPosition;

            Transform[] barRoots = _guests
                .SelectMany(guest => guest.GetComponentsInChildren<Transform>(true))
                .Where(child => child.GetComponent<EmotionBarView>() != null)
                .ToArray();
            _barArrows = FindNamedArrows(barRoots, "Arrow");

            Transform[] viewerRoots = _guests
                .SelectMany(guest => guest.GetComponentsInChildren<Transform>(true))
                .Where(child => child.name == "Viewers (1)")
                .ToArray();
            _viewerArrows = FindNamedArrows(viewerRoots, "Arrow");

            _cardFirstArrows = FindNamedArrows(new[] { _cards.transform }, "Arrow_1");
            _cardSecondArrows = FindNamedArrows(new[] { _cards.transform }, "Arrow_2");
            _timelineFirstArrows = FindNamedArrows(new[] { _timeline.transform }, "Arrow_1");
            _timelineSecondArrows = FindNamedArrows(new[] { _timeline.transform }, "Arrow_2");

            _cardTransforms = Enumerable.Range(0, _cards.transform.childCount)
                .Select(index => _cards.transform.GetChild(index))
                .ToArray();

            _guestFires = _guests
                .SelectMany(guest => guest.GetComponentsInChildren<Transform>(true))
                .Where(child => child.name == "FireFirst" || child.name == "FireSecond")
                .Select(child => child.gameObject)
                .ToArray();

            _firstGuestFire = FindNamedObject(_guests[0], "FireFirst");
            _secondGuestFire = FindNamedObject(_guests[1], "FireSecond");

            _emotionBars = _guests
                .SelectMany(guest => guest.GetComponentsInChildren<EmotionBarView>(true))
                .ToArray();

            SetArrowsActive(System.Array.Empty<RectTransform>());
        }

        private bool ValidateConfiguration()
        {
            if (_tutorContext == null || _tutorContext.TutorSteps == null ||
                _tutorContext.TutorSteps.Count < RequiredStepCount)
            {
                Debug.LogError(
                    $"TutorContext must contain at least {RequiredStepCount} steps.",
                    this);
                _nextButton.interactable = false;
                return false;
            }

            return true;
        }

        private void OnNextButtonClicked()
        {
            if (!_isInitialized)
                return;

            _soundService.Play(SoundType.ButtonClick);
            _soundService.StopVoice();

            if (_currentStep == RequiredStepCount - 1)
            {
                SceneManager.LoadScene(_gameSceneName);
                return;
            }

            _currentStep++;
            ApplyStep(_currentStep);
        }

        private void ApplyStep(int stepIndex)
        {
            StopStepAnimations();
            _nextButtonText.text = stepIndex == RequiredStepCount - 1
                ? StartButtonText
                : NextButtonText;

            switch (stepIndex)
            {
                case 0:
                    SetFocus();
                    break;
                case 1:
                    SetGuestsToIdle();
                    SetFocus(_guests);
                    break;
                case 2:
                    SetGuestsToIdle();
                    SetFocus(_guests);
                    SetArrowsActive(_barArrows);
                    StartBarAnimation();
                    break;
                case 3:
                    SetGuestsToIdle();
                    SetFocus(_guests);
                    SetStepFourBarValues();
                    SetArrowsActive(_viewerArrows);
                    SetStepFourFiresActive();
                    break;
                case 4:
                    SetFocus(_cards);
                    StartPulse(_cardTransforms);
                    break;
                case 5:
                    SetFocus(_cards);
                    SetArrowsActive(_cardFirstArrows);
                    break;
                case 6:
                    SetFocus(_cards);
                    SetArrowsActive(_cardSecondArrows);
                    break;
                case 7:
                    SetFocus(_deckCards);
                    StartPulse(new[] { _deckCards.transform });
                    break;
                case 8:
                    SetFocus(_timeline);
                    SetArrowsActive(_timelineFirstArrows);
                    break;
                case 9:
                    SetFocus(_timeline);
                    SetArrowsActive(_timelineSecondArrows);
                    break;
            }

            TutorStep step = _tutorContext.TutorSteps[stepIndex];
            float replicaDuration = step.VoiceClip != null
                ? Mathf.Max(0.1f, step.VoiceClip.length)
                : float.MaxValue;

            _mainCharacter.ShowStep(step.Text, replicaDuration);

            _soundService.StopVoice();
            if (step.VoiceClip != null)
                _soundService.PlayVoice(step.VoiceClip, null);
        }

        private void SetFocus(params GameObject[] focusedObjects)
        {
            _tint.transform.SetAsLastSibling();

            foreach (GameObject focusedObject in focusedObjects)
                focusedObject.transform.SetAsLastSibling();

            _mainCharacter.transform.SetAsLastSibling();
        }

        private void SetGuestsToIdle()
        {
            foreach (GameObject guest in _guests)
            {
                GuestView guestView = guest.GetComponent<GuestView>();
                if (guestView != null)
                    guestView.PlayIdleAnimation();

                foreach (Animator animator in guest.GetComponentsInChildren<Animator>(true))
                {
                    SetBoolIfPresent(animator, IsTalkHash, false);
                    SetBoolIfPresent(animator, IsCryHash, false);
                }
            }
        }

        private void SetStepFourFiresActive()
        {
            if (_firstGuestFire != null)
                _firstGuestFire.SetActive(true);

            if (_secondGuestFire != null)
                _secondGuestFire.SetActive(true);
        }

        private void SetStepFourBarValues()
        {
            SetGuestBarValue(_guests[0], 40);
            SetGuestBarValue(_guests[1], 70);
        }

        private static void SetGuestBarValue(GameObject guest, int value)
        {
            EmotionBarView emotionBar = guest.GetComponentInChildren<EmotionBarView>(true);
            if (emotionBar != null)
                emotionBar.Initialize(value);
        }

        private void SetGuestFiresActive(bool isActive)
        {
            if (_guestFires == null)
                return;

            foreach (GameObject fire in _guestFires)
                fire.SetActive(isActive);
        }

        private void StartBarAnimation()
        {
            foreach (EmotionBarView emotionBar in _emotionBars)
                emotionBar.Initialize(0);

            _barSequence = DOTween.Sequence()
                .AppendCallback(() => SetBarValues(100))
                .AppendInterval(BarStepDuration)
                .AppendCallback(() => SetBarValues(0))
                .AppendInterval(BarStepDuration)
                .SetLoops(-1)
                .SetUpdate(true);
        }

        private void SetBarValues(int value)
        {
            foreach (EmotionBarView emotionBar in _emotionBars)
                emotionBar.UpdateEmotionBar(value);
        }

        private void StartPulse(IEnumerable<Transform> targets)
        {
            foreach (Transform target in targets)
            {
                Vector3 baseScale = target.localScale;
                _pulseScales[target] = baseScale;
                _pulseTweens[target] = target
                    .DOScale(baseScale * PulseScale, PulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            }
        }

        private void SetArrowsActive(IEnumerable<RectTransform> activeArrows)
        {
            HashSet<RectTransform> activeSet = new(activeArrows);

            foreach (RectTransform arrow in _allArrows)
            {
                StopArrowAnimation(arrow);
                bool isActive = activeSet.Contains(arrow);
                arrow.gameObject.SetActive(isActive);

                if (isActive)
                    StartArrowAnimation(arrow);
            }
        }

        private void StartArrowAnimation(RectTransform arrow)
        {
            Vector2 basePosition = _arrowPositions[arrow];
            arrow.anchoredPosition = basePosition;
            _arrowTweens[arrow] = arrow
                .DOAnchorPosY(basePosition.y + ArrowOffset, ArrowMoveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void StopArrowAnimation(RectTransform arrow)
        {
            if (_arrowTweens.Remove(arrow, out Tween tween))
                tween.Kill();

            if (_arrowPositions.TryGetValue(arrow, out Vector2 basePosition))
                arrow.anchoredPosition = basePosition;
        }

        private void StopStepAnimations()
        {
            _barSequence?.Kill();
            _barSequence = null;

            if (_emotionBars != null)
            {
                foreach (EmotionBarView emotionBar in _emotionBars)
                    emotionBar.Initialize(0);
            }

            foreach ((Transform target, Tween tween) in _pulseTweens)
            {
                tween.Kill();
                if (target != null && _pulseScales.TryGetValue(target, out Vector3 baseScale))
                    target.localScale = baseScale;
            }

            _pulseTweens.Clear();
            _pulseScales.Clear();

            SetGuestFiresActive(false);

            if (_allArrows != null)
                SetArrowsActive(System.Array.Empty<RectTransform>());
        }

        private static RectTransform[] FindNamedArrows(
            IEnumerable<Transform> roots,
            string arrowName)
        {
            return roots
                .SelectMany(root => root.GetComponentsInChildren<RectTransform>(true))
                .Where(rect => rect.name == arrowName)
                .ToArray();
        }

        private static GameObject FindNamedObject(GameObject root, string objectName)
        {
            Transform result = root
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name == objectName);

            return result != null ? result.gameObject : null;
        }

        private static void SetBoolIfPresent(Animator animator, int parameterHash, bool value)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool &&
                    parameter.nameHash == parameterHash)
                {
                    animator.SetBool(parameterHash, value);
                    return;
                }
            }
        }
    }
}
