using Domain.AppSettings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class CryptingService
    {
        private readonly Encryption enc;

        private readonly byte[] _key;
        private readonly byte[] _iv;

        public CryptingService(IOptions<Encryption> settings)
        {
            enc = settings.Value;
            _key = GenerateKey(enc.Key1, enc.Key2);
            _iv = GenerateIV(enc.Key2, enc.Key1);
        }

        private byte[] GenerateKey(string key1, string key2)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(key1 + key2));
            }
        }
        private byte[] GenerateIV(string key1, string key2)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(key2 + key1));
            }
        }
        public async Task<string> CryptText(string? text)
        {
            if(text is  null)
            {
                return "No text";
            }
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(ms, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string DecryptText(string cryptedText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cryptedText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

    }
}
