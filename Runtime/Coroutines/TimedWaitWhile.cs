using System;
using System.Collections;
using UnityEngine;

namespace SensenToolkit
{
    public class TimedWaitWhile
    {
        private const float WaitDelayTime = 0.1f;
        private static readonly WaitForSeconds s_waitDelay = new(WaitDelayTime);
        private static readonly WaitForSecondsRealtime s_waitDelayRealtime = new(WaitDelayTime);
        private readonly Func<bool> _condition;
        private float _timeout;
        private bool _realtime;

        public bool HasTimedout { get; private set; } = false;

        public TimedWaitWhile(Func<bool> condition, float timeout, bool realtime = false)
        {
            _condition = condition;
            _timeout = timeout;
            _realtime = realtime;
        }

        private float GetTime() => _realtime ? Time.realtimeSinceStartup : Time.time;

        public IEnumerator Wait()
        {
            float timeoutMoment = GetTime() + _timeout;
            while (_condition() && GetTime() < timeoutMoment)
            {
                yield return (_realtime ? s_waitDelayRealtime : s_waitDelay);
            }
            if (_timeout <= 0)
            {
                this.HasTimedout = true;
                yield break;
            }
        }
    }
}
