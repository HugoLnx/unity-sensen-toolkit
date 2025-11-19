using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace SensenToolkit
{
    public static class Coroutinesx
    {
        public static TimedWaitWhile TimedWaitWhile(Func<bool> condition, float timeout)
            => new TimedWaitWhile(condition, timeout);
        public static TimedWaitWhile TimedWaitUntil(Func<bool> condition, float timeout)
            => new TimedWaitWhile(() => !condition(), timeout);

        public static void KillAndNullify(MonoBehaviour mono, ref Coroutine coroutine)
        {
            if (coroutine == null) return;
            mono.StopCoroutine(coroutine);
            coroutine = null;
        }

        public static IEnumerator WaitAndExecute(float delay, Action act)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            act?.Invoke();
        }
    }
}
