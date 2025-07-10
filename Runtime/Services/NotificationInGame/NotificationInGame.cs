using System;
using UnityEngine;

namespace SensenToolkit
{
    [Serializable]
    public struct NotificationInGame
    {
        [field: SerializeField] public string Title { get; set; }
        [field: SerializeField] public string Message { get; set; }
        [field: SerializeField] public Sprite Icon { get; set; }
        [field: SerializeField] public string TypeId { get; set; }
    }
}
