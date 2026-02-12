namespace SensenToolkit
{
    public class SaveRepository<TData> where TData : ISaveRootData
    {
        public string Key { get; private set; }

        private SaveRepository(string key)
        {
            Key = key;
        }

        public static SaveRepository<TData> Create(string key)
        {
            SimpleHashing hashing = new();
            return new SaveRepository<TData>(
                key: hashing.SHA1(key)
            );
        }
    }
}
