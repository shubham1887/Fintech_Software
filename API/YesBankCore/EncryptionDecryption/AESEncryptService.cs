using System.Security.Cryptography;
using System.Text;

namespace YesBankCore.EncryptionDecryption
{
    public class AESEncryptService
    {


        // AES Encryption with GCM mode
        public string aesEncrypt(string plainText, string key, string iv)
        {
            using (AesGcm aesGcm = new AesGcm(Encoding.UTF8.GetBytes(key)))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes = new byte[plainBytes.Length + AesGcm.TagByteSizes.MaxSize];
                aesGcm.Encrypt(Encoding.UTF8.GetBytes(iv), plainBytes, cipherBytes.AsSpan(0, plainBytes.Length), cipherBytes.AsSpan(plainBytes.Length));
                return Convert.ToBase64String(cipherBytes);
            }
        }

        // AES Encryption with Padding mode
        public string aesEncryptPadding(string plainText, string key, string iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // PKCS5Padding is equivalent to PKCS7 in .NET

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
        }
        // AES Decryption with GCM mode
        public string aesDecrypt(string cipherText, string key, string iv)
        {
            using (AesGcm aesGcm = new AesGcm(Encoding.UTF8.GetBytes(key)))
            {
                var cipherData = Convert.FromBase64String(cipherText);
                int tagLength = AesGcm.TagByteSizes.MaxSize;
                byte[] encryptedBytes = new byte[cipherData.Length - tagLength];
                byte[] tag = new byte[tagLength];


                Array.Copy(cipherData, encryptedBytes, encryptedBytes.Length);
                Array.Copy(cipherData, encryptedBytes.Length, tag, 0, tagLength);


                byte[] decryptedBytes = new byte[encryptedBytes.Length];
                aesGcm.Decrypt(Encoding.UTF8.GetBytes(iv), encryptedBytes, tag, decryptedBytes);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
            // AES Decryption with Padding mode
            public string aesDecryptPadding(string cipherText, string key, string iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // PKCS5Padding is equivalent to PKCS7 in .NET

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }


            public string randomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }
            return stringBuilder.ToString();
        }


    }
}


