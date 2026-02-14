using System.IO;

namespace SensenToolkit
{
    public class SaveRepository<TData> where TData : ISaveRootData
    {
        private const string DEFAULT_EXTENSION = ".save.dat";

        public string Key { get; private set; }
        private string _filenameKey;
        private string _subfolder;

        private SaveRepository(
            string filenameKey,
            string subfolder = null,
            string extension = DEFAULT_EXTENSION)
        {
            _filenameKey = filenameKey;
            _subfolder = subfolder;
            Key = string.IsNullOrEmpty(subfolder) ? filenameKey : $"{subfolder}/{filenameKey}{extension}";
        }

        public static SaveRepository<TData> Create(
            string filenameKey,
            string subfolder = null,
            string extension = DEFAULT_EXTENSION)
        {
            return new SaveRepository<TData>(
                filenameKey: SimpleHashing.Instance.SHA1Short(filenameKey),
                subfolder: subfolder,
                extension: extension
            );
        }
    }
}
