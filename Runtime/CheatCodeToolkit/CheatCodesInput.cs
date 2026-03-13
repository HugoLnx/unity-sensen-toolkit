using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace SensenToolkit
{
    public class CheatCodesInput : MonoBehaviour
    {
        [SerializeField] private AudioProfile _stingerSfx;
        [SerializeField] private List<CheatCodeConfig> _cheatsList;
        private Dictionary<string, Action> _cheatCodes;
        private Dictionary<string, Action> CheatCodes => _cheatCodes ??= BuildCheatCodeDict();

        private List<TypingCheatCode> _typingCheats = new();
        private StingerService _stingerService;

        private void Awake()
        {
            _stingerService = StingerService.GetInstanceIfExists();
        }

        private void OnEnable()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            keyboard.onTextInput += OnTextInput;
        }

        private void OnDisable()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            keyboard.onTextInput -= OnTextInput;
        }

        private void OnTextInput(char c)
        {
            bool isValid = IsValid(c);
            Debug.Log($"[{nameof(CheatCodesInput)}] {(isValid ? "Received" : "Ignored")} char: '{c}'");
            if (!isValid) return;
            c = char.ToLower(c);

            foreach (string code in CheatCodes.Keys)
            {
                if (c == code[0]) _typingCheats.Add(new TypingCheatCode(code));
            }

            for (int i = _typingCheats.Count - 1; i >= 0; i--)
            {
                TypingCheatCode typingCheat = _typingCheats[i];
                typingCheat.TryTypeChar(c);
                if (typingCheat.IsCompleted)
                {
                    ActivateCheat(typingCheat.Code);
                    _typingCheats.RemoveAt(i);
                    continue;
                }

                if (typingCheat.WasMissTyped)
                {
                    _typingCheats.RemoveAt(i);
                }
            }
        }

        private bool IsValid(char c)
        {
            return char.IsLetterOrDigit(c);
        }

        private void ActivateCheat(string code)
        {
            if (_stingerService != null && _stingerSfx != null)
            {
                _stingerService.Play(_stingerSfx);
            }
            CheatCodes[code].Invoke();
        }

        private Dictionary<string, Action> BuildCheatCodeDict()
        {
            var dict = new Dictionary<string, Action>();
            foreach (CheatCodeConfig cheat in _cheatsList)
            {
                string code = NormalizeCode(cheat.Code);
                if (dict.ContainsKey(code))
                {
                    LogWarning($"Duplicate cheat code: {code}");
                    continue;
                }

                dict.Add(code, () =>
                {
                    if (cheat.Trigger != null) cheat.Trigger.Invoke();
                });
            }
            return dict;
        }

        private string NormalizeCode(string code)
        {
            char[] arr = code.ToCharArray();
            List<char> validChars = new();
            for (int i = 0; i < arr.Length; i++)
            {
                char c = char.ToLower(arr[i]);
                if (IsValid(c)) validChars.Add(c);
                else if (Env.IsDebugBuild)
                {
                    LogWarning($"Cheat code '{code}' contains invalid character '{c}'. It will be ignored.");
                }
            }
            return new string(validChars.ToArray());
        }

        private void LogWarning(string msg)
        {
            if (!Env.IsDebugBuild) return;
            Debug.LogWarning($"[{nameof(CheatCodesInput)}] {msg}");
        }
    }
}
