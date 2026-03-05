using EasyButtons;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class MaskedFillBar : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private Image _fillImg;
        [SerializeField, MustBeAssigned] private Mask _mask;
        [SerializeField] private Direction4.Direction _fillDirection = Direction4.Direction.Right;

        [Button]
        public void SetFillPercent(float percent)
        {
            var direction = Direction4.FromEnum(_fillDirection);
            RectTransform maskRect = _mask.rectTransform;
            RectTransform fillRect = _fillImg.rectTransform;
            float totalLength = direction.MyAxisCoordinate(fillRect.rect.size);

            maskRect.SetSizeWithCurrentAnchors(direction.RectTransformAxis, totalLength * percent);
            fillRect.SetSizeWithCurrentAnchors(direction.RectTransformAxis, totalLength);
        }

        [Button]
        private void EditorSetup(bool forcePivotMiddle = true)
        {
            if (_fillImg.transform.parent != _mask.transform)
            {
                Debug.LogError("Fill image must be a child of the mask.");
                return;
            }
            RectTransform maskRect = _mask.rectTransform;
            RectTransform fillRect = _fillImg.rectTransform;

            var direction = Direction4.FromEnum(_fillDirection);
            if (direction.IsVertical)
            {
                float x = forcePivotMiddle ? 0.5f : maskRect.pivot.x;
                maskRect.pivot = new Vector2(x, direction.IsUp ? 0 : 1);
            }
            else if (direction.IsHorizontal)
            {
                float y = forcePivotMiddle ? 0.5f : maskRect.pivot.y;
                maskRect.pivot = new Vector2(direction.IsRight ? 0 : 1, y);
            }
            fillRect.pivot = maskRect.pivot;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMax = Vector2.zero;
            fillRect.offsetMin = Vector2.zero;
        }

        [Button]
        private void PrintReport()
        {
            var direction = Direction4.FromEnum(_fillDirection);
            string report = "MaskedFillBar Report:\n"
            + "\nFill\n"
            + $"fill.sizeDelta: {direction.MyAxisCoordinate(_fillImg.rectTransform.sizeDelta)}\n"
            + $"fill.rectSize: {direction.MyAxisCoordinate(_fillImg.rectTransform.rect.size)}\n"
            + $"fill.offsetMin: {direction.MyAxisCoordinate(_fillImg.rectTransform.offsetMin)}\n"
            + $"fill.offsetMax: {direction.MyAxisCoordinate(_fillImg.rectTransform.offsetMax)}\n"
            + "\nMask\n"
            + $"mask.sizeDelta: {direction.MyAxisCoordinate(_mask.rectTransform.sizeDelta)}\n"
            + $"mask.rectSize: {direction.MyAxisCoordinate(_mask.rectTransform.rect.size)}\n"
            + $"mask.offsetMin: {direction.MyAxisCoordinate(_mask.rectTransform.offsetMin)}\n"
            + $"mask.offsetMax: {direction.MyAxisCoordinate(_mask.rectTransform.offsetMax)}";
            Debug.Log(report);
        }
    }
}
