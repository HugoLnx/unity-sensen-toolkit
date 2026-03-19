using System.Collections;
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
        private PointerEventData _data;
        private bool _isMouseOver = false;
        private float MouseWheelDeltaY => Mouse.current.scroll.ReadValue().y;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _data = EventSystem.current == null ? null
                : new PointerEventData(EventSystem.current);
            _isMouseOver = true;
            StopAllCoroutines();
            StartCoroutine(LoopScrollCheck());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        private IEnumerator LoopScrollCheck()
        {
            while (_isMouseOver && EventSystem.current != null && _data != null)
            {
                _scrolledThisFrame = false;
                if (_isMouseOver && IsMouseWheelRolling())
                {
                    float scrollInput = MouseWheelDeltaY;
                    float scrollY = Time.deltaTime * scrollSensitivity * (scrollInput < 0 ? -1f : 1f);
                    _data.scrollDelta = new Vector2(0f, scrollY);

                    OnScroll(_data);
                }
                yield return null;
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
