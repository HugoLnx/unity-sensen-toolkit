namespace SensenToolkit
{
    public class InputBlockService : AMultiHolderHubService<InputBlockService, object>
    {
        private const string APPLICATION_FOCUS_KEY = "APPLICATION_FOCUS";
        private const string APPLICATION_PAUSE_KEY = "APPLICATION_PAUSE";
        public bool IsInputBlocked => IsHeld;

        private void OnApplicationFocus(bool focus)
        {
            if (focus) Release(APPLICATION_FOCUS_KEY);
            else Hold(APPLICATION_FOCUS_KEY);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) Hold(APPLICATION_PAUSE_KEY);
            else Release(APPLICATION_PAUSE_KEY);
        }
    }
}
