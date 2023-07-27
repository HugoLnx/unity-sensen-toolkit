using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace SensenToolkit
{
    public static class TaskExtensionMethods
    {
        public static IEnumerator WaitForCompletion(this Task task)
        {
            return new WaitUntil(() => task.IsCompleted);
        }

        public static ConfiguredTaskAwaitable<T> AwaitInCurrentThread<T>(this Task<T> task)
        {
            return task.ConfigureAwait(true);
        }
        public static ConfiguredTaskAwaitable AwaitInCurrentThread(this Task task)
        {
            return task.ConfigureAwait(true);
        }

        public static ConfiguredTaskAwaitable<T> AwaitInAnyThread<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
        public static ConfiguredTaskAwaitable AwaitInAnyThread(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static async Task<T> DelayWhile<T>(this Task<T> task, Func<bool> predicate)
        {
            while (predicate())
            {
                await Task.Delay(300).AwaitInAnyThread();
            }
            return await task.AwaitInCurrentThread();
        }
        public static async Task DelayWhile(this Task task, Func<bool> predicate)
        {
            await task
            .ToTypedTask()
            .DelayWhile<bool>(predicate)
            .AwaitInCurrentThread();
        }

        public static async Task<T> DelayUntil<T>(this Task<T> task, Func<bool> predicate)
        {
            while (!predicate())
            {
                await Task.Delay(300).AwaitInAnyThread();
            }
            return await task.AwaitInCurrentThread();
        }
        public static async Task DelayUntil(this Task task, Func<bool> predicate)
        {
            await task
            .ToTypedTask()
            .DelayUntil<bool>(predicate)
            .AwaitInCurrentThread();
        }

        public static async Task<T> RetryOnError<T>(this Task<T> task, int maxRetries = 3, float delay = 0.1f, float delayIncrement = 1f)
        {
            for (int i = 0; i < maxRetries - 1; i++)
            {
                try
                {
                    return await task.AwaitInCurrentThread();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    await Task.Delay((int)(delay * 1000)).AwaitInAnyThread();
                    delay += delayIncrement;
                }
            }

            // Last try
            await Task.Delay((int)(delay * 1000)).AwaitInAnyThread();
            return await task.AwaitInCurrentThread();
        }

        public static async Task RetryOnError(this Task task, int maxRetries = 3, float delay = 0.1f, float delayIncrement = 1f)
        {
            await task
            .ToTypedTask()
            .RetryOnError<bool>(maxRetries, delay, delayIncrement)
            .AwaitInCurrentThread();
        }
        public static Task<T> DelayWhileOffline<T>(this Task<T> task)
        {
            return task.DelayWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
        }
        public static Task DelayWhileOffline(this Task task)
        {
            return task
            .ToTypedTask()
            .DelayWhileOffline<bool>();
        }

        private static Task<bool> ToTypedTask(this Task task)
        {
            return task.ContinueWith(_ => true, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
