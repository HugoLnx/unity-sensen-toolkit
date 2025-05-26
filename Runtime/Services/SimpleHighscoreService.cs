using System;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace Bumashuta
{
    public class SimpleHighscoreService : APermanentSingleton<SimpleHighscoreService>
    {
        private const string HIGH_SCORE_KEY = "Highscore";
        [SerializeField] private float _forceSaveDelaySecs = 60f;
        [SerializeField] private bool _saveOnPrefs = true;
        [SerializeField, ReadOnly] private int _highscore;
        private SimpleTimer _saveThrottlingTimer;

        public int Highscore => _highscore;

        public event Action<int> OnHighscoreChanged;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            _highscore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            if (_saveOnPrefs)
            {
                _saveThrottlingTimer = new SimpleTimer(_forceSaveDelaySecs);
                _saveThrottlingTimer.OnEnd += ForceSave;
            }
        }

        public void ForceSave()
        {
            if (!_saveOnPrefs) return;
            PlayerPrefs.Save();
        }

        public void TryUpdateValue(int value, bool overwrite = false)
        {
            if (!overwrite && _highscore >= value) return;
            _highscore = value;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, _highscore);
            OnHighscoreChanged?.Invoke(_highscore);

            if (_saveOnPrefs && !_saveThrottlingTimer.IsRunning)
            {
                ForceSave();
                _saveThrottlingTimer.Restart();
            }
        }
    }
}
