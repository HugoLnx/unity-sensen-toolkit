using System;
using EasyButtons;
using MyBox;
using SensenToolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SensenToolkit
{
    public class SimpleHighscoreService : APermanentSingleton<SimpleHighscoreService>
    {
        private const string HIGH_SCORE_KEY = "Highscore";
        [SerializeField] private float _forceSaveDelaySecs = 60f;
        [SerializeField] private bool _saveOnPrefs = true;
        [SerializeField, ReadOnly] private int _highscore;
        private SimpleTimer _saveThrottlingTimer;
        private bool _wasInitialized = false;
        private bool _emittedNewHighscore = false;

        public int Highscore => _highscore;

        public event Action<int> OnHighscoreChanged = delegate { };
        public event Action OnNewHighscore = delegate { };
        public event Action OnInit = delegate { };

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            _highscore = _saveOnPrefs ? PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0) : 0;
            if (_saveOnPrefs)
            {
                _saveThrottlingTimer = new SimpleTimer(_forceSaveDelaySecs);
                _saveThrottlingTimer.OnEnd += ForceSave;
            }
            _wasInitialized = true;
            BootEmittedHighscore();
            OnInit.Invoke();
        }

        private void OnEnable()
        {
            AppCore.OnSceneLoadEnd += OnSceneLoadEnd;
            _emittedNewHighscore = false;
        }

        protected override void OnDisableAny()
        {
            base.OnDisableSingleton();
            AppCore.OnSceneLoadEnd -= OnSceneLoadEnd;
        }

        private void OnSceneLoadEnd(Scene _)
        {
            BootEmittedHighscore();
        }

        private void BootEmittedHighscore()
        {
            if (_highscore == 0)
            {
                // If it hasn't been set yet, we don't emit a new highscore.
                _emittedNewHighscore = true;
                return;
            }
            _emittedNewHighscore = false;
        }

        public void ForceSave()
        {
            if (!_saveOnPrefs) return;
            PlayerPrefs.Save();
        }

        public void AddInitializationListener(Action action)
        {
            if (_wasInitialized) action.Invoke();
            OnInit += action;
        }

        public void RemoveInitializationListener(Action action)
        {
            OnInit -= action;
        }

        public void BootWithValue(int value)
        {
            _highscore = value;
            if (_saveOnPrefs)
            {
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, _highscore);
            }
            BootEmittedHighscore();
            OnHighscoreChanged.Invoke(_highscore);
        }

        public void TryUpdateValue(int value, bool overwrite = false)
        {
            if (!overwrite && _highscore >= value) return;
            int previousHighscore = _highscore;
            _highscore = value;

            if (_saveOnPrefs)
            {
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, _highscore);
            }
            OnHighscoreChanged.Invoke(_highscore);
            if (_highscore > previousHighscore && !_emittedNewHighscore)
            {
                _emittedNewHighscore = true;
                OnNewHighscore.Invoke();
            }

            if (_saveOnPrefs && !_saveThrottlingTimer.IsRunning)
            {
                ForceSave();
                _saveThrottlingTimer.Restart();
            }
        }

        [Button]
        public void ResetHighscore()
        {
            _highscore = 0;
            if (_saveOnPrefs)
            {
                PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
            }
            OnHighscoreChanged.Invoke(_highscore);
            BootEmittedHighscore();
        }
    }
}
