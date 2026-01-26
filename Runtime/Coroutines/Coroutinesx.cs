using System;
using System.Collections;
using UnityEngine;

namespace SensenToolkit
{
    public static class Coroutinesx
    {
        public static IEnumerator TimedWaitWhile(Func<bool> condition, float timeout)
            => new TimedWaitWhile(condition, timeout).Wait();
        public static IEnumerator TimedWaitUntil(Func<bool> condition, float timeout)
            => new TimedWaitWhile(() => !condition(), timeout).Wait();

        public static void KillAndNullify(MonoBehaviour mono, ref Coroutine coroutine)
        {
            if (coroutine == null) return;
            mono.StopCoroutine(coroutine);
            coroutine = null;
        }

        public static IEnumerator WaitAndExecute(float delay, Action act, bool unscaled = false)
        {
            if (delay > 0f)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            act?.Invoke();
        }
    }
}
