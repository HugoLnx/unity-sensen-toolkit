using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SensenToolkit
{
    public class SimpleTimer
    {
        private float _durationSecs;

        public bool IsStopped => !IsRunning;
        public bool HasEnded { get; private set; } = false;
        public bool IsRunning => !IsPaused && RemainingTime > 0;
        public bool IsPaused => _delayedResetCancellation == null && RemainingTime > 0;
        public float RemainingTime { get; private set; } = 0f;

        private CancellationTokenSource _delayedResetCancellation;

        public event Action OnEnd;

        public SimpleTimer(float durationSecs = 1f)
        {
            _durationSecs = durationSecs;
        }

        public SimpleTimer Restart(float? durationSecs = null)
        {
            HasEnded = false;
            RemainingTime = durationSecs ?? _durationSecs;
            if (RemainingTime <= 0) throw new InvalidOperationException("Timer duration must be set on constructor or Restart method");
            Resume();
            return this;
        }

        public SimpleTimer Stop()
        {
            HasEnded = false;
            Pause();
            RemainingTime = 0f;
            return this;
        }

        public SimpleTimer Reset() => Stop();

        public SimpleTimer Resume()
        {
            if (_delayedResetCancellation != null) return this;
            if (RemainingTime <= 0) throw new InvalidOperationException("Timer is not running");

            _delayedResetCancellation = new CancellationTokenSource();
            UniTask.Void(DelayedReset, _delayedResetCancellation.Token);
            return this;
        }

        public SimpleTimer Pause()
        {
            _delayedResetCancellation?.Cancel();
            _delayedResetCancellation = null;
            return this;
        }

        private async UniTaskVoid DelayedReset(CancellationToken cancelToken)
        {
            while (RemainingTime > 0)
            {
                await UniTask.NextFrame(cancelToken);
                RemainingTime -= Time.deltaTime;
            }
            Stop();
            HasEnded = true;
            OnEnd?.Invoke();
        }

        public void SetDurationSecs(float durationSecs)
        {
            _durationSecs = durationSecs;
        }
    }
}
