using UnityEngine;

namespace SensenToolkit
{
    public class CursorUnlocker : MonoBehaviour
    {
        private void Start()
        {
            CursorUtils.UnlockCursor();
        }
    }
}
