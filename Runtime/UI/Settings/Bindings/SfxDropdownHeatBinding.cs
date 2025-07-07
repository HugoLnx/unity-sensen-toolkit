#if SENSEN_UI_HEAT
using MyBox;
using UnityEngine;
using Michsky.UI.Heat;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace SensenToolkit
{
    public class SfxDropdownHeatBinding : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, AutoProperty] private Dropdown _dropdown;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxOpen;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxClose;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxSelect;
        [Tooltip("Sfx to play when the dropdown is disabled. If null, no sound will be played when the dropdown is disabled.")]
        [SerializeField] private AudioProfile _sfxWhenDisabled;
        private SfxService _sfxService;
        private EventTrigger.Entry _clickOutsideEvtEntry;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
            _clickOutsideEvtEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick,
            };
            _clickOutsideEvtEntry.callback.AddListener(OnClickOutside);
        }

        private IEnumerator Start()
        {
            if (!_dropdown.enableTrigger) yield break;
            while (_dropdown.triggerObject == null || _dropdown.triggerObject.GetComponent<EventTrigger>() == null)
            {
                yield return null;
            }
            _dropdown.triggerObject.GetComponent<EventTrigger>().triggers.Add(_clickOutsideEvtEntry);
        }

        private void OnEnable()
        {
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDisable()
        {
            _dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }

        private void OnClickOutside(BaseEventData _)
        {
            if (!_dropdown.isInteractable) return;

            _sfxService.Play(_sfxClose);
        }

        private void OnDropdownValueChanged(int _)
        {
            if (!_dropdown.isInteractable) return;
            _sfxService.Play(_sfxSelect);
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (!_dropdown.isInteractable)
            {
                if (_sfxWhenDisabled != null) _sfxService.Play(_sfxWhenDisabled);
                return;
            }

            if (_dropdown.isOn) _sfxService.Play(_sfxOpen);
        }
    }
}
#endif
