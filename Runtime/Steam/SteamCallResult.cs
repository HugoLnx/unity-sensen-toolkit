using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SensenToolkit
{
    public struct SteamCallResult<T>
    {
        public T Value;
        public bool IsError;
        public readonly bool IsSuccess => !IsError && Value != null;
    }

    public class SteamCallHandler<T>
    where T : struct
    {
        private CallResult<T> _callback;
        private SteamCallResult<T>? _result = null;
        private SteamAPICall_t? _handle = null;

        public void SetHandle(SteamAPICall_t handle)
        {
            if (_handle.HasValue) throw new System.Exception($"Wait for current handle before setting a new one");
            _handle = handle;
        }

        public IEnumerator WaitForResult(SteamAPICall_t? handle = null, bool discardResult = false)
        {
#if ENABLESTEAMWORKS
            if (!SteamManager.IsFunctional) throw new System.Exception($"Steam is not initialized");
            if (!_handle.HasValue && !handle.HasValue) throw new System.Exception($"No handle to wait for");
            if (_handle.HasValue && handle.HasValue)
            {
                yield return new WaitUntil(() => !_handle.HasValue);
            }
            _handle = handle ?? _handle.Value;
            _callback ??= CallResult<T>.Create(OnCall);
            _callback.Set(_handle.Value);
            yield return new WaitUntil(() => _result.HasValue);
            if (discardResult) _result = null;
            _handle = null;
#else
            Debug.LogWarning($"SteamCallHandler is not functional because Steamworks is not enabled");
            yield break;
#endif
        }

        public SteamCallResult<T> PopResult()
        {
            if (!_result.HasValue) throw new System.Exception($"There's no result to pop");
            SteamCallResult<T> r = _result.Value;
            _result = null;
            return r;
        }

        private void OnCall(T value, bool failure)
        {
            if (failure)
            {
                Debug.LogWarning($"Steam Call Failure {this}");
                _result = new SteamCallResult<T> { IsError = true };
            }
            else
            {
                _result = new SteamCallResult<T> { Value = value };
            }
        }
    }
}
