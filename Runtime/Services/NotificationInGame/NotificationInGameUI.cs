#if DOTWEEN
using System.Collections;
using DG.Tweening;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class NotificationInGameUI : MonoBehaviour
    {
        [SerializeField] private float _fadeDistance = 30f;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _visibleDuration = 4.5f;
        [SerializeField] private AudioProfile _sfxFadeIn;
        [SerializeField] private AudioProfile _sfxFadeOut;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Image _iconImg;
        [SerializeField] private CanvasGroup _canvasGroup;
        private SfxService _sfxService;
        private Tween _tween;
        private Vector2 _initialPosition;
        private RectTransform Rect => (RectTransform)_canvasGroup.transform;
        public bool IsShowing { get; private set; }

        private Vector2 OutPosition => _initialPosition.With(x: _initialPosition.x + _fadeDistance);

        private void Awake()
        {
            _sfxService = SfxService.Instance;
            _initialPosition = Rect.anchoredPosition;
            SetupFade();
        }

        private void OnDisable()
        {
            Tweenx.KillAndNullify(ref _tween);
            IsShowing = false;
            _canvasGroup.alpha = 0f;
        }

        [Button]
        public void Show(NotificationInGame notification)
        {
            Assertx.IsNotNull(notification, "Notification cannot be null");
            Assertx.IsNotNull(_titleText, "Title text cannot be null");
            Assertx.IsNotNull(_messageText, "Message text cannot be null");
            Assertx.IsNotNull(_iconImg, "Icon image cannot be null");

            StartCoroutine(NotificationCoroutine(notification));
        }

        private IEnumerator NotificationCoroutine(NotificationInGame notification)
        {
            _sfxService.Play(_sfxFadeIn);
            Tweenx.KillAndNullify(ref _tween);

            IsShowing = true;
            SetupFade();

            _titleText.text = notification.Title;
            _messageText.text = notification.Message;
            _iconImg.sprite = notification.Icon;

            _tween = FadeIn();

            yield return _tween.WaitForKill();

            yield return new WaitForSeconds(_visibleDuration);
            _tween = FadeOut();
            _sfxService.Play(_sfxFadeOut);
            yield return _tween.WaitForKill();
            IsShowing = false;
        }

        private Tween FadeIn()
        => DOTween.Sequence()
            .Join(DOTween.To(
                getter: () => Rect.anchoredPosition,
                setter: v => Rect.anchoredPosition = v,
                endValue: _initialPosition,
                duration: _fadeInDuration
            ).SetEase(Ease.OutSine))
            .Join(Tweenx.FromZeroToOne(
                action: v => _canvasGroup.alpha = v,
                duration: _fadeInDuration
            ).SetEase(Ease.InOutSine))
            .Play();

        private Tween FadeOut()
        => DOTween.Sequence()
            .Join(DOTween.To(
                getter: () => Rect.anchoredPosition,
                setter: v => Rect.anchoredPosition = v,
                endValue: OutPosition,
                duration: _fadeOutDuration
            ).SetEase(Ease.InSine))
            .Join(Tweenx.FromOneToZero(
                action: v => _canvasGroup.alpha = v,
                duration: _fadeOutDuration
            ).SetEase(Ease.InOutSine))
            .Play();


        private void SetupFade()
        {
            Rect.anchoredPosition = OutPosition;
            _canvasGroup.alpha = 0f;
        }
    }
}
#endif
