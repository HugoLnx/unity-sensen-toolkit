using System.Linq;
using SysCrypto = System.Security.Cryptography;

namespace SensenToolkit
{
    public class SimpleHashing
    {
        private static SimpleHashing _instance;
        public static SimpleHashing Instance => _instance ??= new();
        private SysCrypto.MD5 _md5;
        private SysCrypto.SHA1 _sha1;
        private SysCrypto.SHA256 _sha256;
        private SysCrypto.SHA512 _sha512;
        private SysCrypto.MD5 MD5Obj => _md5 ??= SysCrypto.MD5.Create();
        private SysCrypto.SHA1 SHA1Obj => _sha1 ??= SysCrypto.SHA1.Create();
        private SysCrypto.SHA256 SHA256Obj => _sha256 ??= SysCrypto.SHA256.Create();
        private SysCrypto.SHA512 SHA512Obj => _sha512 ??= SysCrypto.SHA512.Create();

        public string MD5(string key) => HashWith(MD5Obj, key);
        public string SHA1(string key) => HashWith(SHA1Obj, key);
        public string SHA256(string key) => HashWith(SHA256Obj, key);
        public string SHA512(string key) => HashWith(SHA512Obj, key);
        public string SHA1Short(string key) => SHA1(key).Substring(0, 9);

        private static string HashWith(SysCrypto.HashAlgorithm hashAlg, string key)
        {
            return ToHexString(hashAlg.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key)));
        }

        private static string ToHexString(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }
}
