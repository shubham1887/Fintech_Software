using Newtonsoft.Json;
using RestSharp;
using System.Net;
using YesBankCore.EncryptionDecryption;
using YesBankCore.Model;
using YesBankCore.Response;
using YESBankCore.Controller;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YesBankCore.Controller
{
    internal class SORHTTPProcessing
    {
        private SORErrorProcessing log = new SORErrorProcessing();

        private RestRequest GetRequest()
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        public NetworkResponse Post(string url, string plainText, string serivceName)
        {
            NetworkResponse response = new NetworkResponse();

            try
            {
                var client = new RestClient(url)
                {
                    ReadWriteTimeout = 5000 * 1000
                };

                string requestStr = plainText.Trim();


                RestRequest request = GetRequest();

                log.saveRequest("URL", url, serivceName);
                log.saveRequest("Plain Request", requestStr, serivceName);

                YesBankCore.Response.EncryptDecryptService.EncryptResponse encrypt = new EncryptDecryptService()
                    .encryptRequest(KYCCredentials.PartnerKey, requestStr, KYCCredentials.PrivateKey, KYCCredentials.BankPublicKey, serivceName);

                var requestBody = new { body = encrypt.encryptRequestBody };
                string requestJSON = JsonConvert.SerializeObject(requestBody).Trim();

                log.saveRequest("key", encrypt.encryptedSecretKey, serivceName);
                log.saveRequest("token", encrypt.requestToken, serivceName);
                log.saveRequest("partner", encrypt.encryptedPartnerKey, serivceName);
                log.saveRequest("iv", encrypt.plainRequestIv, serivceName);
                log.saveRequest("encryptRequestBody", encrypt.encryptRequestBody, serivceName);

                request.AddHeader("key", encrypt.encryptedSecretKey);
                request.AddHeader("token", encrypt.requestToken);
                request.AddHeader("partner", encrypt.encryptedPartnerKey);
                request.AddHeader("iv", encrypt.plainRequestIv);

                request.AddJsonBody(requestJSON);

                IRestResponse result = client.Execute(request);
                log.saveRequest("response", result.Content, serivceName);

                APIResponse responses = JsonConvert.DeserializeObject<APIResponse>(result.Content);

                if (result.Content == null || result.Content.Length <= 0)
                {
                    response.Status = Message.FAILURE;
                    response.Response = result?.ErrorException.ToString();
                    return response;
                }

                if (responses.status.EqualsIgnoreCase("ERROR"))
                {
                    response.Status = Message.FAILURE;
                    response.Response = responses.responseMessage.ToString();
                    return response;
                }

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    response.Status = Message.PROCESS;
                    APIError res = JsonConvert.DeserializeObject<APIError>(result.Content);
                    response.Response = $"{res?.httpCode}, {res?.httpMessage}, {res?.moreInformation}";
                    return response;
                }

                string? responseKey = result.Headers.FirstOrDefault(h => h.Name == "key")?.Value.ToString();
                string? responseIv = result.Headers.FirstOrDefault(h => h.Name == "iv")?.Value.ToString();
                string? responseHash = result.Headers.FirstOrDefault(h => h.Name == "hash")?.Value.ToString();

                log.saveRequest("key", responseKey, serivceName);
                log.saveRequest("token", responseIv, serivceName);
                log.saveRequest("hash", responseHash, serivceName);
                APIResponse aPIResponse = JsonConvert.DeserializeObject<APIResponse>(result.Content);

                string Response = new EncryptDecryptService().decryptResponse(responseHash, responseIv, responseKey, aPIResponse.body, KYCCredentials.PrivateKey, KYCCredentials.BankPublicKey, serivceName);



                response.Status = result.StatusCode.ToString();
                response.Response = Response;
                log.saveRequest("plain Response", response.Response, serivceName);

                return response;
            }
            catch (Exception ex)
            {
                response.Status = Message.PROCESS;
                response.Response = ex.Message.ToString();
                log.saveRequest("Exception ", ex.Message, serivceName);
            }

            return response;
        }
    }
}
