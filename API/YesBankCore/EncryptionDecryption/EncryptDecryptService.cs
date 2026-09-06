using EncryptionLibrary;
using System.Security.Cryptography;
using YesBankCore.Controller;
using YESBankCore.Controller;

namespace YesBankCore.EncryptionDecryption
{
    public class EncryptDecryptService
    {
        private SORErrorProcessing log = new SORErrorProcessing();
        public Response.EncryptDecryptService.EncryptResponse encryptRequest(string plainPartnerKey, string plainRequestBody, string partnerPrivateKey, string bankPublicKey, string ServiceName)
        {

            Response.EncryptDecryptService.EncryptResponse response = new Response.EncryptDecryptService.EncryptResponse();
            RSAEncryptService rsaInstance = new RSAEncryptService();
            AESEncryptService aesInstance = new AESEncryptService();
            string plainSecretKey = "";
            if (ServiceName.EqualsIgnoreCase(YesBankCore.Controller.ServiceName.Payout))
            {
                plainSecretKey = aesInstance.randomString(16);
                response.plainRequestIv = aesInstance.randomString(16);
            }
            else
            {
                plainSecretKey = aesInstance.randomString(32);
                response.plainRequestIv = aesInstance.randomString(12);
            }


            string bankPublicKeyStr = File.ReadAllText(bankPublicKey);
            string partnerPrivateKeyStr = File.ReadAllText(partnerPrivateKey);

            log.saveRequest("Symmetric Key",plainSecretKey, ServiceName);
            RSAParameters publicKey = rsaInstance.readPublicKey(bankPublicKeyStr);
            RSAParameters privateKey = rsaInstance.readPrivateKeyPKCS8(partnerPrivateKeyStr);
            response.encryptedPartnerKey = rsaInstance.rsaEncrypt(plainPartnerKey, publicKey);
            response.encryptedSecretKey = rsaInstance.rsaEncrypt(plainSecretKey, publicKey);
            response.requestToken = rsaInstance.signData(plainRequestBody, privateKey);
            if (ServiceName.EqualsIgnoreCase(YesBankCore.Controller.ServiceName.Payout))
            {
                response.encryptRequestBody = aesInstance.aesEncryptPadding(plainRequestBody, plainSecretKey, response.plainRequestIv);
            }
            else
            {
                response.encryptRequestBody = aesInstance.aesEncrypt(plainRequestBody, plainSecretKey, response.plainRequestIv);
            }

            return response;
        }
        public string decryptResponse(string responseHash, string responseIv, string responseSecretKey, string responseBody, string partnerPrivateKey, string bankPublicKey, string ServiceName)
        {
            RSAEncryptService rsaInstance = new RSAEncryptService();
            AESEncryptService aesInstance = new AESEncryptService();

            string bankPublicKeyStr = File.ReadAllText(bankPublicKey);
            string partnerPrivateKeyStr = File.ReadAllText(partnerPrivateKey);

            RSAParameters publicKey = rsaInstance.readPublicKey(bankPublicKeyStr);
            RSAParameters privateKey = rsaInstance.readPrivateKeyPKCS8(partnerPrivateKeyStr);
            string plainSecretKey = rsaInstance.rsaDecrypt(responseSecretKey, privateKey);
            string response = "";
            bool isVerified;
            if (ServiceName.EqualsIgnoreCase(YesBankCore.Controller.ServiceName.Payout))
            {
                response = aesInstance.aesDecryptPadding(responseBody, plainSecretKey, responseIv);
                isVerified = rsaInstance.verifyData(response, responseHash, publicKey);

            }
            else
            {
                response = aesInstance.aesDecrypt(responseBody, plainSecretKey, responseIv);
                isVerified = rsaInstance.verifyData(response, responseHash, publicKey);
            }

            return response;
        }
    }
}

