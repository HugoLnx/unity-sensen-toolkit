#if DOTWEEN
using DG.Tweening;
using UnityEngine;

namespace SensenToolkit
{
    public class DOTweenBoot : APermanentSingleton<DOTweenBoot>
    {
        [SerializeField] private int _tweensCapacity = 200;
        [SerializeField] private int _sequenceCapacity = 50;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            DOTween.SetTweensCapacity(_tweensCapacity, _sequenceCapacity);
        }
    }
}
#endif
