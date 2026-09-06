using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace YESBankCore.Controller
{
    internal class EncryptDecrypt
    {
        private static char[] hexDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        internal byte[] HexfromString(string s)
        {
            int i = s.Length;
            byte[] abyte0 = new byte[(i + 1) / 2];
            int j = 0;
            int k = 0;
            if (i % 2 == 1)
            {
                abyte0[k++] = (byte)HexfromDigit(s[j++]);
            }
            while (j < i)
            {
                abyte0[k++] = (byte)(HexfromDigit(s[j++]) << 4 | HexfromDigit(s[j++]));
            }
            return abyte0;
        }

        internal int HexfromDigit(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }
            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }
            throw new ArgumentException("invalid hex digit: " + c);
        }

        internal string asHex(byte[] buf)
        {
            StringBuilder strbuf = new StringBuilder(buf.Length * 2);
            for (int i = 0; i < buf.Length; i++)
            {
                if ((buf[i] & 0xFF) < 16)
                {
                    strbuf.Append("0");
                }
                strbuf.Append(buf[i].ToString("X2"));
            }
            return strbuf.ToString();
        }

        internal string HextoString(byte[] abyte0, int i, int j)
        {
            char[] ac = new char[j * 2];
            int k = 0;
            for (int l = i; l < i + j; l++)
            {
                byte byte0 = abyte0[l];
                ac[k++] = hexDigits[byte0 >> 4 & 0xF];
                ac[k++] = hexDigits[byte0 & 0xF];
            }
            return new string(ac);
        }

        internal string HextoString(byte[] abyte0)
        {
            return HextoString(abyte0, 0, abyte0.Length);
        }

        public string Encrypt(string key, string message)
        {
            try
            {
                var skeySpec = new RijndaelManaged { Key = HexfromString(key) };
                var cipher = new RijndaelManaged();
                cipher.Key = skeySpec.Key;
                cipher.Mode = CipherMode.ECB;
                cipher.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptCipher = cipher.CreateEncryptor();

                byte[] encstr = encryptCipher.TransformFinalBlock(Encoding.UTF8.GetBytes(message), 0, message.Length);
                return HextoString(encstr);
            }
            catch (Exception ex)
            {
                new ErrorProcessing().saveRequest("initEncrypt", ex.Message);
            }
            return "";
        }

        public string Decrypt(string dec_key, string message)
        {
            try
            {
                var skeySpec = new RijndaelManaged();
                skeySpec.Key = HexfromString(dec_key);
                skeySpec.Mode = CipherMode.ECB;
                skeySpec.Padding = PaddingMode.PKCS7;

                var decryptor = skeySpec.CreateDecryptor();

                byte[] encstr = HexfromString(message);
                byte[] decstr = decryptor.TransformFinalBlock(encstr, 0, encstr.Length);
                return Encoding.UTF8.GetString(decstr);
            }
            catch (CryptographicException ce)
            {
                throw new Exception("Invalid Key or input String");
            }
        }

        public string AesEncrypt(string plainText, string encryptionKey)
        {
            string base64EncryptedString = "";

            // Create a SHA-256 hash of the secret key
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digestOfPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

                // Create key and IV from the hash
                byte[] keyBytes = new byte[24];
                byte[] iv = new byte[16];
                Array.Copy(digestOfPassword, keyBytes, 24);  // 24 bytes for the AES key
                Array.Copy(digestOfPassword, iv, 16);        // 16 bytes for the IV

                // Encrypt the message using AES with CBC mode and PKCS7 padding
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    // Encrypt the plain text message
                    byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes;

                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cs.FlushFinalBlock();
                            encryptedBytes = ms.ToArray();
                        }
                    }

                    // Base64 encode the encrypted message
                    base64EncryptedString = Convert.ToBase64String(encryptedBytes);
                }
            }

            return base64EncryptedString;
        }

        public string AesDecrypt(string cipherText, string encryptionKey)
        {

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digestOfPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

                // Create key bytes (192 bits) and IV (128 bits)
                byte[] keyBytes = new byte[24];
                byte[] iv = new byte[16];
                Array.Copy(digestOfPassword, keyBytes, 24);
                Array.Copy(digestOfPassword, iv, 16);

                // Create AES key and IV
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = keyBytes;
                    aesAlg.IV = iv;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    // Decrypt the message
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                        using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    return srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        public string Encryption(string message, string secretKey)
        {
            message= message.Trim();
            secretKey = secretKey.Trim();
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digestOfPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
                byte[] keyBytes = new byte[24];
                byte[] iv = new byte[16];
                Array.Copy(digestOfPassword, keyBytes, 24);
                Array.Copy(digestOfPassword, iv, 16);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] plainTextBytes = Encoding.UTF8.GetBytes(message);
                        byte[] cipherTextBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                        return Convert.ToBase64String(cipherTextBytes);
                    }
                }
            }
        }

        public string Decryption(string base64EncryptedString, string secretKey)
        {
            string decryptedString = "";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digestOfPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
                byte[] keyBytes = new byte[24];
                byte[] iv = new byte[16];
                Array.Copy(digestOfPassword, keyBytes, 24);
                Array.Copy(digestOfPassword, iv, 16);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(base64EncryptedString);
                        byte[] plainTextBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        decryptedString = Encoding.UTF8.GetString(plainTextBytes);
                    }
                }
            }
            return decryptedString;
        }

        public string CreateMD5(string Password)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}