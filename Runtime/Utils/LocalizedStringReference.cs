using UnityEngine.Localization;

namespace SensenToolkit
{
    public class LocalizedStringReference
    {
        private LocalizedString _current;
        private object[] _args;
        private string Text => _current?.GetLocalizedString(_args) ?? string.Empty;
        private event System.Action<string> OnUpdateText = delegate { };

        public void AddTextListener(System.Action<string> onUpdateText)
        {
            OnUpdateText += onUpdateText;
            EmitUpdate();
        }

        public void RemoveTextListener(System.Action<string> onUpdateText)
        {
            OnUpdateText -= onUpdateText;
        }

        public void SetLocalized(LocalizedString str, params object[] args)
        {
            if (_current == str && args.Length == 0) return;
            LocalizedString newStr = str;
            LocalizedString oldStr = _current;
            _current = str;
            SetArgs(args);

            if (oldStr != newStr)
            {
                oldStr.StringChanged -= OnStringChanged;
            }
            if (newStr != null)
            {
                newStr.StringChanged += OnStringChanged;
            }
            OnUpdateText.Invoke(Text);
        }

        public void SetArgs(object[] args)
        {
            _args = args;
            _current?.RefreshString();
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
