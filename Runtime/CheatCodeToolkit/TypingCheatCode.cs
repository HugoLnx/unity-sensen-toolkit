using UnityEngine;


namespace SensenToolkit
{
    public class TypingCheatCode
    {
        public string Code { get; }
        public int LastTypedInx { get; set; }
        public bool IsCompleted => !WasMissTyped && LastTypedInx >= Code.Length - 1;
        public bool WasMissTyped { get; private set; }

        public TypingCheatCode(string code)
        {
            Code = code;
            LastTypedInx = -1;
        }

        public void TryTypeChar(char c)
        {
            if (IsCompleted || WasMissTyped) return;
            int nextInx = Mathf.Clamp(LastTypedInx + 1, 0, Code.Length - 1);

            if (c == Code[nextInx])
            {
                LastTypedInx = nextInx;
            }
            else
            {
                WasMissTyped = true;
            }
        }
    }
}
