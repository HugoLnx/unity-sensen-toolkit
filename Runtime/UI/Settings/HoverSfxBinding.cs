using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SensenToolkit
{
    public class HoverSfxBinding : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField, MustBeAssigned] private AudioProfile _hoverSfx;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _sfxService.Play(_hoverSfx);
        }
    }
}
