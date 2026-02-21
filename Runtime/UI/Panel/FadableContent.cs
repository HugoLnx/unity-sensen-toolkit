#if DOTWEEN
using System;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadableContent : MonoBehaviour
    {
        [SerializeField, AutoProperty] private CanvasGroup _canvasGroup;
        [SerializeField, AutoProperty(AutoPropertyMode.Parent, allowEmpty: true)]
        private PanelFadable _parentPanel;
        [SerializeField] private float _fadeDuration = 0.25f;
        private Queue<Action> _changesQueue = new();
        private Tween _tween;

        private bool IsHidden => _canvasGroup.alpha <= 0f || (_parentPanel != null && !_parentPanel.IsVisible);
        public bool HasChangesToApply => _changesQueue.Count > 0;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void InstantShow()
        {
            Tweenx.KillAndNullify(ref _tween);
            ApplyQueuedChangesIfHidden();
            SetAlpha(1f);
        }

        public void InstantHide()
        {
            Tweenx.KillAndNullify(ref _tween);
            SetAlpha(0f);
            ApplyQueuedChanges();
        }

        public void ShowFading()
        {
            Tweenx.KillAndNullify(ref _tween);
            ApplyQueuedChangesIfHidden();
            float valueDiff = 1f - _canvasGroup.alpha;
            _tween = Tweenx.FromTo(
                action: SetAlpha,
                duration: _fadeDuration * valueDiff,
                startValue: _canvasGroup.alpha,
                endValue: 1f
            )
            .SetUpdate(isIndependentUpdate: true)
            .SetEase(Ease.InOutSine);
        }

        public void HideFading()
        {
            Tweenx.KillAndNullify(ref _tween);
            float valueDiff = _canvasGroup.alpha;
            _tween = Tweenx.FromTo(
                action: SetAlpha,
                duration: _fadeDuration * valueDiff,
                startValue: _canvasGroup.alpha,
                endValue: 0f
            )
            .SetUpdate(isIndependentUpdate: true)
            .SetEase(Ease.InOutSine)
            .OnComplete(ApplyQueuedChanges);
        }

        public void HideAndReshowFading()
        {
            Tweenx.KillAndNullify(ref _tween);
            float valueDiffToHide = _canvasGroup.alpha;
            _tween = DOTween.Sequence()
            .Append(Tweenx.FromTo(
                    action: SetAlpha,
                    duration: _fadeDuration * valueDiffToHide,
                    startValue: _canvasGroup.alpha,
                    endValue: 0f
                )
                .SetEase(Ease.InOutSine)
            )
            .AppendCallback(ApplyQueuedChanges)
            .Append(Tweenx.FromTo(
                    action: SetAlpha,
                    duration: _fadeDuration,
                    startValue: 0f,
                    endValue: 1f
                )
                .SetEase(Ease.InOutSine)
            )
            .SetUpdate(isIndependentUpdate: true)
            .Play();
        }

        public void ApplyWhenHidden(Action changeAction)
        {
            if (changeAction == null) return;
            _changesQueue.Enqueue(changeAction);
            ApplyQueuedChangesIfHidden();
        }

        public void ClearChangesQueue()
        {
            _changesQueue.Clear();
        }

        private void ApplyQueuedChanges()
        {
            while (_changesQueue.Count > 0)
            {
                Action changeAction = _changesQueue.Dequeue();
                changeAction.Invoke();
            }
        }

        private void ApplyQueuedChangesIfHidden()
        {
            if (!IsHidden) return;
            ApplyQueuedChanges();
        }

        private void SetAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }
    }
}
#endif
