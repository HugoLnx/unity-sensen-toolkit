using System.IO;

namespace SensenToolkit
{
    public class SaveRepository<TData> where TData : ISaveRootData
    {
        public string Key { get; private set; }
        private string _filenameKey;
        private string _subfolder;

        private SaveRepository(string filenameKey, string subfolder = null)
        {
            _filenameKey = filenameKey;
            _subfolder = subfolder;
            Key = string.IsNullOrEmpty(subfolder) ? filenameKey : $"{subfolder}/{filenameKey}";
        }

        public static SaveRepository<TData> Create(string filenameKey, string subfolder = null)
        {
            SimpleHashing hashing = new();
            return new SaveRepository<TData>(
                filenameKey: hashing.SHA1Short(filenameKey),
                subfolder: subfolder
            );
        }
    }
}
