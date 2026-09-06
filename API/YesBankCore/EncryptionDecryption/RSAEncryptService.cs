using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;


namespace EncryptionLibrary
{
    public class RSAEncryptService
    {
        public string rsaEncrypt(string plainText, RSAParameters publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                rsa.ImportParameters(publicKey);
                byte[] cipherBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);// false for OAEP padding, true for PKCS1 padding
                return Convert.ToBase64String(cipherBytes);
            }
        }


        // RSA Decryption with private key
        public string rsaDecrypt(string cipherText, RSAParameters privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                rsa.ImportParameters(privateKey);
                byte[] plainBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);// false for OAEP padding, true for PKCS1 padding
                return Encoding.UTF8.GetString(plainBytes);
            }
        }


        public RSAParameters readPrivateKeyPKCS1(string pemPriKey)
        {
            using (TextReader textReader = new StringReader(pemPriKey))
            {
                var pemReader = new PemReader(textReader);
                AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                RsaPrivateCrtKeyParameters priKeyParameters = (RsaPrivateCrtKeyParameters)keyPair.Private;
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(priKeyParameters);
                return rsaParameters;
            }
        }


        public RSAParameters readPrivateKeyPKCS8(string pemPriKey)
        {
            using (TextReader textReader = new StringReader(pemPriKey))
            {
                var pemReader = new PemReader(textReader);
                var priKeyParams = pemReader.ReadObject() as RsaPrivateCrtKeyParameters;
                if (priKeyParams != null)
                {
                    RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(priKeyParams);
                    return rsaParameters;
                }
                else
                {
                    throw new Exception("Invalid private key format.");
                }
            }
        }


        public RSAParameters readPublicKey(string pemPubKey)
        {
            using (TextReader textReader = new StringReader(pemPubKey))
            {
                var pemReader = new PemReader(textReader);
                RsaKeyParameters keyParameters = (RsaKeyParameters)pemReader.ReadObject();
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(keyParameters);
                return rsaParameters;
            }
        }


        // Generate digital signature
        public string signData(string data, RSAParameters privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                byte[] signBytes = rsa.SignData(Encoding.UTF8.GetBytes(data), new SHA1CryptoServiceProvider());
                return Convert.ToBase64String(signBytes);
            }
        }


        // Verify digital signature
        public bool verifyData(string data, string signature, RSAParameters publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                return rsa.VerifyData(Encoding.UTF8.GetBytes(data), new SHA1CryptoServiceProvider(), Convert.FromBase64String(signature));
            }
        }


    }


}


