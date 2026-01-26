using System;
using System.Collections;
using UnityEngine;

namespace SensenToolkit
{
    public class TimedWaitWhile
    {
        private const float WaitDelayTime = 0.1f;
        private static readonly WaitForSeconds s_waitDelay = new(WaitDelayTime);
        private readonly Func<bool> _condition;
        private float _timeout;
        public bool HasTimedout { get; private set; } = false;

        public TimedWaitWhile(Func<bool> condition, float timeout)
        {
            this._condition = condition;
            this._timeout = timeout;
        }

        public IEnumerator Wait()
        {
            float timeoutMoment = Time.time + _timeout;
            while (_condition() && Time.time < timeoutMoment)
            {
                yield return s_waitDelay;
            }
            if (_timeout <= 0)
            {
                this.HasTimedout = true;
                yield break;
            }
        }
    }
}
