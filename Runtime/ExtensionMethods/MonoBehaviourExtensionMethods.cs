using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SensenToolkit
{
    public static class MonoBehaviourExtensionMethods
    {
        public static void TryStopCoroutine(this MonoBehaviour mono, ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                mono.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public static IEnumerator StartCoroutinesInParallel(this MonoBehaviour mono, params IEnumerator[] enumerators)
        {
            return StartCoroutinesInParallel(mono, (IEnumerable<IEnumerator>) enumerators);
        }

        public static IEnumerator StartCoroutinesInParallel(this MonoBehaviour mono, IEnumerable<IEnumerator> enumerators)
        {
            List<Coroutine> coroutines = enumerators
                .Select(coroutine => mono.StartCoroutine(coroutine))
                .ToList();

            foreach (Coroutine coroutine in coroutines)
            {
                yield return coroutine;
            }
        }

        public static Coroutine StartTaskAsCoroutine(this MonoBehaviour mono, Task task)
        {
            return mono.StartCoroutine(task.WaitForCompletion());
        }

        public static Task StartCoroutineAsTask(this MonoBehaviour mono, IEnumerator coroutine)
        {
            TaskCompletionSource<bool> tcs = new();
            mono.StartCoroutine(CoroutineAsync(coroutine, tcs));
            return tcs.Task;
        }

        public static async Task<T> StartCoroutineAsTask<T>(this MonoBehaviour mono, System.Func<TaskCompletionSource<T>, IEnumerator> coroutineExec)
        {
            TaskCompletionSource<bool> tcs = new();
            TaskCompletionSource<T> tcsResult = new();
            mono.StartCoroutine(CoroutineAsync(coroutineExec.Invoke(tcsResult), tcs));
            await tcs.Task.AwaitInCurrentThread();
            return tcsResult.Task.Result;
        }

        private static IEnumerator CoroutineAsync(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
        {
            yield return coroutine;
            tcs.SetResult(true);
        }
    }
}
