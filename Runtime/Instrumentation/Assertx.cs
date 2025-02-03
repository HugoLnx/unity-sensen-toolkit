using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SensenToolkit
{
    public static class Assertx
    {
        [Conditional("UNITY_EDITOR")]
        public static void IsNotEmpty<T>(IReadOnlyCollection<T> collection, string message = null)
        {
            IsNotNull(collection, "collection is null");
            if (collection.Count == 0)
            {
                throw new InvalidOperationException(FormatMessage(message, "collection is empty"));
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsNotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                throw new NullReferenceException(FormatMessage(message, "object is null"));
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsTrue(bool v, string message)
        {
            if (!v)
            {
                throw new InvalidOperationException(FormatMessage(message, "condition is false"));
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsFalse(bool v, string message)
        {
            if (v)
            {
                throw new InvalidOperationException(FormatMessage(message, "condition is true"));
            }
        }

        private static string FormatMessage(string message, string defaultMessage)
        {
            return message ?? $"Assetx failed: {defaultMessage}";
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsNormalized(Vector3 direction, bool acceptZero = true)
        {
            float sqrMag = direction.sqrMagnitude;
            bool isNormalized = Mathf.Approximately(sqrMag, 1f) || (acceptZero && Mathf.Approximately(sqrMag, 0f));
            IsTrue(isNormalized, "Vector3 is not normalized");
        }

        [Conditional("UNITY_EDITOR")]
        public static void IsNormalized(Vector2 direction, bool acceptZero = true)
        {
            IsNormalized(new Vector3(direction.x, direction.y, 0), acceptZero);
        }

        public static void OnlyOneHasValue(params object[] objects)
        {
            int count = 0;
            foreach (object obj in objects)
            {
                if (obj != null)
                {
                    count++;
                }
            }
            IsTrue(count == 1, "Only should have value");
        }

        public static void IsNotZero(Vector3 vec, string message = null)
        {
            IsTrue(vec != Vector3.zero, message ?? "Vector3 shouldn't be zero");
        }
        public static void IsNotZero(Vector2 vec, string message = null)
        {
            IsTrue(vec != Vector2.zero, message ?? "Vector2 shouldn't be zero");
        }
        public static void IsNotZero(float value, string message = null)
        {
            IsTrue(!Mathf.Approximately(value, 0f), message ?? "Value shouldn't be zero");
        }

        public static void IsDifferent(Vector2 a, Vector2 b, string message = null)
        {
            IsTrue((a - b).sqrMagnitude > Mathf.Epsilon, message ?? "Vectors shouldn't be equal");
        }

        public static void IsNotMagnitudeZero(Vector3 vec)
        {
            IsTrue(vec.sqrMagnitude > Mathf.Epsilon, "Vector3 shouldn't have zero magnitude");
        }
    }
}
