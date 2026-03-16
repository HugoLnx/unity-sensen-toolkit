using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class RadioButtonGroup : MonoBehaviour
    {
        [SerializeField] private bool _canBeEmpty = false;
        [SerializeField] private bool _canSelectMultiple = false;
        [SerializeField] private List<int> _defaultActiveIndexes = new() { 0 };
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private RadioButton[] _radioButtons;
        private List<int> _activeIndexes = new();
        private List<RadioButton> _activeButtons = new();

        public IReadOnlyList<int> ActiveIndexes => _activeIndexes;
        public int ActiveIndex => _activeIndexes.Count == 0 ? -1 : _activeIndexes[0];
        public RadioButton ActiveButton => ActiveIndex >= 0 ? _radioButtons[ActiveIndex] : null;
        public IReadOnlyList<RadioButton> ActiveButtons => _activeButtons;
        private bool _btnBlockOperation = false;

        public Action OnStateChanged = delegate { };

        private void Start()
        {
            Reinitialize(findButtons: false);
        }

        private void OnEnable() => RebindAllButtons();
        private void OnDisable() => UnbindAllButtons();

        public void Reinitialize(bool findButtons = true)
        {
            if (findButtons) _radioButtons = GetComponentsInChildren<RadioButton>();
            RebindAllButtons();
            _activeIndexes.Clear();
            _activeButtons.Clear();
            foreach (RadioButton btn in _radioButtons)
            {
                btn.Reset();
            }
            SwitchToDefaults();
        }

        public void SwitchToDefaults() => SwitchTo(-1);
        public void SwitchTo(RadioButton btn, bool turnOn = true)
        {
            EnforceOperation(btn, turnOn);
        }

        public void SwitchTo(int index, bool turnOn = true)
        {
            if (index < 0 || index >= _radioButtons.Length)
            {
                index = -1;
            }

            EnforceOperation(index == -1 ? null : _radioButtons[index], turnOn);
        }

        private void EnforceOperation(RadioButton btn, bool turnOn)
        {
            if (_btnBlockOperation) return;

            _btnBlockOperation = true;
            SwitchButtonsToEnforceOperation(btn, turnOn);
            _btnBlockOperation = false;

            RefreshGroupState();
            OnStateChanged?.Invoke();
        }

        private void SwitchButtonsToEnforceOperation(RadioButton btn, bool turnOn)
        {
            foreach (RadioButton otherBtn in _radioButtons)
            {
                otherBtn.ShouldDeactivateOnClick = _canSelectMultiple || _canBeEmpty;
            }

            bool turnOff = !turnOn;
            if (btn == null)
            {
                if (_canBeEmpty) SwitchAllOff();
                else SwitchToDefaultIndexes();
                return;
            }

            int activeCount = 0;
            foreach (RadioButton otherBtn in _radioButtons)
            {
                if (otherBtn.IsActive) activeCount++;
            }

            bool willBecomeEmpty = turnOff && (activeCount == 0 || (activeCount == 1 && btn.IsActive));
            if (!_canBeEmpty && willBecomeEmpty)
            {
                SwitchToDefaultIndexes();
                return;
            }

            if (!_canSelectMultiple && turnOn)
            {
                SwitchOnExclusively(btn);
                return;
            }

            btn.SwitchTo(turnOn);
        }

        private void SwitchAllOff()
        {
            foreach (RadioButton otherBtn in _radioButtons)
            {
                otherBtn.SwitchTo(false);
            }
        }

        private void SwitchToDefaultIndexes()
        {
            HashSet<int> defaultIndexes;
            if (!_canBeEmpty && _defaultActiveIndexes.Count == 0)
            {
                defaultIndexes = new HashSet<int> { 0 };
            }
            else if (!_canSelectMultiple && _defaultActiveIndexes.Count > 1)
            {
                defaultIndexes = new HashSet<int> { _defaultActiveIndexes[0] };
            }
            else
            {
                defaultIndexes = new HashSet<int>(_defaultActiveIndexes);
            }
            foreach (int inx in defaultIndexes)
            {
                _radioButtons[inx].SwitchTo(defaultIndexes.Contains(inx));
            }
        }

        private void SwitchOnExclusively(RadioButton btn)
        {
            foreach (RadioButton otherBtn in _radioButtons)
            {
                otherBtn.SwitchTo(btn == otherBtn);
            }
        }

        private void RefreshGroupState()
        {
            _activeIndexes.Clear();
            _activeButtons.Clear();
            for (int i = 0; i < _radioButtons.Length; i++)
            {
                if (_radioButtons[i].IsActive)
                {
                    _activeIndexes.Add(i);
                    _activeButtons.Add(_radioButtons[i]);
                }
            }
        }

        private void RebindAllButtons()
        {
            UnbindAllButtons();
            foreach (RadioButton button in _radioButtons)
            {
                button.OnStateChanged += EnforceOperation;
            }
        }

        private void UnbindAllButtons()
        {
            foreach (RadioButton button in _radioButtons)
            {
                button.OnStateChanged -= EnforceOperation;
            }
        }
    }
}
