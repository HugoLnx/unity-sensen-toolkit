using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Localization;
using MyBox;
using System.Collections.Generic;

namespace SensenToolkit
{
    public class QuickLoadingTextAnimation : MonoBehaviour
    {
        [SerializeField] private LocalizedString _loadingI18n;
        [SerializeField] private float _loopDurationSecs = 1f;
        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibilityEvents;
        [SerializeField, AutoProperty] private TMP_Text _text;
        private List<string> _textStates = null;
        private Coroutine _animationCoroutine;

        private void Awake()
        {
            _visibilityEvents.OnShow += OnShow;
            _visibilityEvents.OnHidden += OnHidden;
        }

        private void OnShow() => EnsureTextAnimation();
        private void OnHidden() => StopTextAnimation();

        private void EnsureTextAnimation()
        {
            // Debug.Log($"[{nameof(QuickLoadingTextAnimation)}] Show{" hasCoroutine".If(_animationCoroutine != null)}");
            if (_animationCoroutine != null) return;
            string loadingStr = _loadingI18n.GetLocalizedString();
            loadingStr = Regex.Replace(loadingStr, @"\.", "");
            _textStates = new string[] { "", ".", "..", "..." }
                .Select(dots => $"{loadingStr}{dots}")
                .ToList();
            _animationCoroutine = StartCoroutine(TextAnimationCoroutine());
        }

        private IEnumerator TextAnimationCoroutine()
        {
            WaitForSecondsRealtime waitBetweenFrames = new(_loopDurationSecs / _textStates.Count);
            int inx = 0;
            while (_textStates != null)
            {
                _text.text = _textStates[inx];
                inx = (inx + 1) % _textStates.Count;
                yield return waitBetweenFrames;
            }
        }

        private void StopTextAnimation()
        {
            // Debug.Log($"[{nameof(QuickLoadingTextAnimation)}] Hide{" hasCoroutine".If(_animationCoroutine != null)}");
            Coroutinesx.KillAndNullify(this, ref _animationCoroutine);
            _textStates = null;
        }
    }
}
