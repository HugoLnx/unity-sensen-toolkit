using System;
using MyBox;
using SensenToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class LeaderboardEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, MustBeAssigned] private TMP_Text _rankingText;
        [SerializeField, MustBeAssigned] private TMP_Text _nicknameText;
        [SerializeField, MustBeAssigned] private TMP_Text _scoreText;
        [SerializeField, MustBeAssigned] private FadableContent _hoverHighlight;
        [SerializeField] private bool _boldOnPlayerEntry = true;
        [SerializeField] private bool _underlineOnPlayerEntry = true;
        [SerializeField] private bool _changeColorOnPlayerEntry = true;
        [SerializeField, ConditionalField(nameof(_changeColorOnPlayerEntry))]
        private Color _playerEntryColor = Color.yellow;
        private LeaderboardEntry? _entry;
        private FontStyles _originalFontStyle;
        private Color _originalFontColor;
        private bool _hasStarted;

        public static LeaderboardEntryUI Instantiate(LeaderboardEntryUI prefab, RectTransform parent, LeaderboardEntry entry)
        {
            LeaderboardEntryUI instance = Instantiate(prefab, parent);
            instance.SetEntry(entry);
            return instance;
        }

        private void Start()
        {
            _originalFontStyle = _nicknameText.fontStyle;
            _originalFontColor = _nicknameText.color;
            _hasStarted = true;
            TryEnforceEntryStyling();
        }

        private void OnEnable()
        {
            _hoverHighlight.InstantHide();
        }

        public void SetEntry(LeaderboardEntry entry)
        {
            _entry = entry;
            TryEnforceEntryStyling();
        }

        private void TryEnforceEntryStyling()
        {
            if (!_hasStarted || !_entry.HasValue) return;
            LeaderboardEntry entry = _entry.Value;
            _rankingText.text = entry.Ranking.ToString();
            _nicknameText.text = entry.Nickname;
            _scoreText.text = entry.FormatScore();

            FontStyles style = entry.IsPlayerEntry
                ? GetPlayerFontStyle()
                : _originalFontStyle;
            _rankingText.fontStyle = style;
            _nicknameText.fontStyle = style;
            _scoreText.fontStyle = style;


            Color color = entry.IsPlayerEntry && _changeColorOnPlayerEntry
                ? _playerEntryColor
                : _originalFontColor;
            _rankingText.color = color;
            _nicknameText.color = color;
            _scoreText.color = color;
        }

        private FontStyles GetPlayerFontStyle()
        {
            FontStyles style = FontStyles.Normal;
            if (_boldOnPlayerEntry) style |= FontStyles.Bold;
            if (_underlineOnPlayerEntry) style |= FontStyles.Underline;
            return style;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoverHighlight.ShowFading();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverHighlight.HideFading();
        }
    }
}
