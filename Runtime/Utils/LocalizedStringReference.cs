using System;
using UnityEngine.Localization;

namespace SensenToolkit
{

    public class LocalizedStringReference
    {
        private LocalizedString _current;
        private object[] _args;
        private string Text => LocStringx.IsPresent(_current)
            ? _current.GetLocalizedString(_args)
            : null;
        private event System.Action<string> OnUpdateText = delegate { };

        public void AddTextListener(System.Action<string> onUpdateText)
        {
            OnUpdateText += onUpdateText;
            if (!string.IsNullOrEmpty(Text))
            {
                onUpdateText.Invoke(Text);
            }
        }

        public void RemoveTextListener(System.Action<string> onUpdateText)
        {
            OnUpdateText -= onUpdateText;
        }

        public void SetLocalized(LocalizedString newStr, params object[] args)
        {
            bool isCurrentPresent = LocStringx.IsPresent(_current);
            bool isNewPresent = LocStringx.IsPresent(newStr);

            bool strChanged = isNewPresent != isCurrentPresent
                || (isNewPresent && newStr != _current);
            if (strChanged)
            {
                LocalizedString oldStr = _current;
                _current = newStr;

                if (LocStringx.IsPresent(oldStr))
                {
                    oldStr.StringChanged -= OnStringChanged;
                }
                if (LocStringx.IsPresent(newStr))
                {
                    newStr.StringChanged += OnStringChanged;
                }
            }

            bool currentHasArgs = _args != null && _args.Length > 0;
            bool newHasArgs = args != null && args.Length > 0;
            bool argsChanged = newHasArgs || currentHasArgs || (newHasArgs != currentHasArgs);
            if (argsChanged)
            {
                SetArgs(args, emitUpdate: false);
            }
            if (strChanged || argsChanged) EmitUpdate();
        }

        public void SetArgs(object[] args, bool emitUpdate = true)
        {
            _args = args;
            if (LocStringx.IsPresent(_current))
            {
                _current.RefreshString();
                if (emitUpdate) EmitUpdate();
            }
        }

        private void OnStringChanged(string value)
        {
            OnUpdateText.Invoke(value);
        }

        private void EmitUpdate()
        {
            OnUpdateText.Invoke(Text);
        }
    }
}
