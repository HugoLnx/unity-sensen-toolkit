using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    public struct OnOffSpritePair
    {
        [SerializeField, MustBeAssigned] public Sprite OnSprite;
        [SerializeField, MustBeAssigned] public Sprite OffSprite;

        public Sprite GetSprite(bool isOn) => isOn ? OnSprite : OffSprite;
    }
}
