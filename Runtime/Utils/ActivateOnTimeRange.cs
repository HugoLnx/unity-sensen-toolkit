using System;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    public class DateTimeConfig
    {
        public int Day;
        public int Month;
        public int Year;
        public int Hour;
        public int Minute;
        public int Second;
        public int TimeZoneOffset;

        public bool IsValid()
        {
            try
            {
                ToDateTime();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second).AddHours(-TimeZoneOffset);
        }
    }

    [RequireComponent(typeof(PanelChildVisibilityEvents))]
    public class ActivateOnTimeRange : MonoBehaviour
    {
        [SerializeField] private DateTimeConfig _startTime;
        [SerializeField] private DateTimeConfig _endTime;
        [SerializeField] private bool _simulateNowDate;
        [SerializeField, ConditionalField(nameof(_simulateNowDate))]
        private DateTimeConfig _nowDate;
        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibilityEvents;

#if UNITY_EDITOR
        public DateTime Now => _simulateNowDate && _nowDate.IsValid() ? _nowDate.ToDateTime() : DateTime.UtcNow;
#else
        public DateTime Now => DateTime.UtcNow;
#endif

        private void Awake()
        {
            _visibilityEvents.OnShow += RefreshActive;
        }

        private void OnEnable()
        {
            RefreshActive();
        }

        private void RefreshActive()
        {
            gameObject.SetActive(IsInTimeRange());
        }

        private bool IsInTimeRange()
        {
            DateTime now = Now;
            DateTime startTime = _startTime.IsValid() ? _startTime.ToDateTime() : DateTime.MinValue;
            DateTime endTime = _endTime.IsValid() ? _endTime.ToDateTime() : DateTime.MaxValue;
            if (startTime > endTime)
            {
                Debug.LogError("Start time must be before end time.");
                return false;
            }
            return now >= startTime && now <= endTime;
        }

    }
}
