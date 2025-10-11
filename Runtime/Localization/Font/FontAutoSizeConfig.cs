namespace SensenToolkit
{
    [System.Serializable]
    public struct FontAutoSizeConfig
    {
        public bool Enabled;
        public float MinSize;
        public float MaxSize;

        public FontAutoSizeConfig(bool enabled, float minSize, float maxSize)
        {
            Enabled = enabled;
            MinSize = minSize;
            MaxSize = maxSize;
        }
    }
}
