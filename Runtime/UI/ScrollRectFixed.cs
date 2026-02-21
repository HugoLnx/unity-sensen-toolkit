using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SensenToolkit
{
    // TODO: Maybe it's not useful?
    public class ScrollRectFixed : ScrollRect, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _scrolledThisFrame = true;
        private bool _isMouseOver = false;
        private float MouseWheelDeltaY => Mouse.current.scroll.ReadValue().y;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        private void Update()
        {
            _scrolledThisFrame = false;
            if (_isMouseOver && IsMouseWheelRolling())
            {
                float scrollInput = MouseWheelDeltaY;
                PointerEventData data = new(EventSystem.current);
                float scrollY = Time.deltaTime * scrollSensitivity * (scrollInput < 0 ? -1f : 1f);
                data.scrollDelta = new Vector2(0f, scrollY);

                OnScroll(data);
            }
        }

        public override void OnScroll(PointerEventData data)
        {
            if (_scrolledThisFrame) return;
            base.OnScroll(data);
            _scrolledThisFrame = true;
        }

        private bool IsMouseWheelRolling()
        {
            return !Mathf.Approximately(MouseWheelDeltaY, 0);
        }

    }
}
