using System;
using System.Collections.Generic;
using EasyButtons;

namespace SensenToolkit
{
    public class NotificationInGameService : ATransientSingleton<NotificationInGameService>
    {
        private readonly List<NotificationInGame> _queue = new();
        public bool HasAny => _queue.Count > 0;

        public event Action OnNotificationAdded = delegate { };

        [Button]
        public void Notify(NotificationInGame notification)
        {
            if (!string.IsNullOrWhiteSpace(notification.TypeId))
            {
                for (int i = _queue.Count - 1; i >= 0; i--)
                {
                    if (_queue[i].TypeId == notification.TypeId)
                    {
                        _queue[i] = notification;
                        OnNotificationAdded.Invoke();
                        return;
                    }
                }
            }

            _queue.Add(notification);
            OnNotificationAdded.Invoke();
        }

        public void RemoveNotificationOfType(string typeId)
        {
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (_queue[i].TypeId == typeId)
                {
                    _queue.RemoveAt(i);
                }
            }
        }

        public bool TryPopNext(out NotificationInGame notification)
        {
            if (_queue.Count == 0)
            {
                notification = default;
                return false;
            }

            notification = _queue[0];
            _queue.RemoveAt(0);
            return true;

        }

        public void AddListener(Action action)
        {
            Assertx.IsNotNull(action, "Action cannot be null");
            if (_queue.Count > 0) action();
            OnNotificationAdded += action;
        }

        public void RemoveListener(Action action)
        {
            Assertx.IsNotNull(action, "Action cannot be null");
            OnNotificationAdded -= action;
        }
    }
}
