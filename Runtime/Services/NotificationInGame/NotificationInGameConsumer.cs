#if DOTWEEN
using System.Collections;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class NotificationInGameConsumer : MonoBehaviour
    {
        [SerializeField, AutoProperty] private NotificationInGameUI _notificationUI;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private NotificationInGameService _notifications;
        private Coroutine _consumerCoroutine;

        private void Awake()
        {
            _notifications = NotificationInGameService.Instance;
        }

        private void OnEnable()
        {
            _consumerCoroutine = null;
            _notifications.AddListener(EnsureRunningConsumer);
        }

        private void OnDisable()
        {
            _notifications.RemoveListener(EnsureRunningConsumer);
            _consumerCoroutine = null;
        }

        private void EnsureRunningConsumer()
        {
            if (_consumerCoroutine != null) return;
            _consumerCoroutine = StartCoroutine(ConsumerCoroutine());
        }

        private IEnumerator ConsumerCoroutine()
        {
            yield return new WaitWhile(() => _notificationUI.IsShowing);
            while (_notifications.TryPopNext(out NotificationInGame notification))
            {
                _notificationUI.Show(notification);
                yield return new WaitWhile(() => _notificationUI.IsShowing);
            }
            _consumerCoroutine = null;
        }
    }
}
#endif
