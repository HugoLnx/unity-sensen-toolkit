using System;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class ShallowLinkInfo
    {
        public readonly string Id;
        public readonly string Text;

        // Be careful with this reference, link info is reused internally by TMPro
        public readonly TMP_LinkInfo OriginalInfo;

        public ShallowLinkInfo(TMP_LinkInfo linkInfo)
        {
            Id = linkInfo.GetLinkID();
            Text = linkInfo.GetLinkText();
            OriginalInfo = linkInfo;
        }
    }

    public class InteractiveTextLinks : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, AutoProperty] private TMP_Text _text;
        private ShallowLinkInfo _hoveredLink;
        private bool _isHoveringText;

        public event Action<ShallowLinkInfo> OnLinkClicked = delegate { };
        public event Action<ShallowLinkInfo> OnLinkHovered = delegate { };
        public event Action<ShallowLinkInfo> OnLinkUnhovered = delegate { };

        private void OnDisable()
        {
            UnhoverLink();
        }

        private void Update()
        {
            if (!_isHoveringText) return;
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, mousePosition, null);
            ShallowLinkInfo previousLink = _hoveredLink;
            if (linkIndex == -1)
            {
                if (previousLink == null) return;
                _hoveredLink = null;
                OnLinkUnhovered.Invoke(previousLink);
                return;
            }

            ShallowLinkInfo linkInfo = new(_text.textInfo.linkInfo[linkIndex]);
            bool hasLinkChanged = previousLink?.Id != linkInfo.Id;
            if (hasLinkChanged)
            {
                if (previousLink != null) OnLinkUnhovered.Invoke(previousLink);
                OnLinkHovered.Invoke(linkInfo);
                _hoveredLink = linkInfo;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_hoveredLink == null) return;
            OnLinkClicked.Invoke(_hoveredLink);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHoveringText = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHoveringText = false;
            UnhoverLink();
        }

        private void UnhoverLink()
        {
            if (_hoveredLink == null) return;
            OnLinkUnhovered.Invoke(_hoveredLink);
            _hoveredLink = null;
        }
    }
}
