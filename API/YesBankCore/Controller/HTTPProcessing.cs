using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using YESBankCore.Model;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YESBankCore.Controller
{
    internal class HTTPProcessing
    {
        private ErrorProcessing log = new ErrorProcessing();

        private RestRequest GetRequest()
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("X-IBM-Client-Secret", Credential.XIBMClientSecret);
            request.AddHeader("X-IBM-Client-ID", Credential.XIBMClientID);
            //string text = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(Credential.AuthUserName + ":" + Credential.AuthPassword));
            //request.AddHeader("Authorization", "Basic " + text);
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        private void SetCertificate(RestClient client)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();

                string file = Path.Combine(Directory.GetCurrentDirectory(), Credential.PrivateKeyFilePath).Trim();
                byte[] data = File.ReadAllBytes(file);

                X509Certificate2 certificate = new X509Certificate2(data, Credential.PrivateKeyPassword,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                x509Certificate2Collection.Add(certificate);
                client.ClientCertificates = new X509CertificateCollection();
                client.ClientCertificates.AddRange(x509Certificate2Collection);
            }
            catch (Exception ex)
            {
                log.saveRequest("setCertificate Exception ", ex.Message);
            }
        }

        public NetworkResponse Post(string url, string plainText)
        {
            NetworkResponse response = new NetworkResponse();

            try
            {
                var client = new RestClient(url);
                client.ReadWriteTimeout = 5000 * 1000;
                SetCertificate(client);
                string requestStr = plainText;

                RestRequest request = GetRequest();

                log.saveRequest("URL", url);

                log.saveRequest("Payload", requestStr);

                string EncryptedJsonPayload = new EncryptDecrypt().Encrypt(Credential.MerchantKey, requestStr);

                APIRequest aPIRequest = new APIRequest();
                aPIRequest.requestMsg = EncryptedJsonPayload;
                aPIRequest.pgMerchantId = Credential.MerchantId;

                string dataPayload = JsonConvert.SerializeObject(aPIRequest);

                log.saveRequest("dataPayload", dataPayload);

                request.AddParameter("application/json", dataPayload, ParameterType.RequestBody);

                IRestResponse result = client.Execute(request);

                response.Status = Message.OK;

                if (result.Content == null || result.Content.Length <= 0)
                {
                    response.Status = Message.FAILURE;
                    response.Response = result.ErrorException.ToString();
                    return response;
                }

                if (result.Content.EqualsIgnoreCase("Internal Server Error"))
                {
                    response.Status = Message.FAILURE;
                    response.Response = result.Content + " " + result.ErrorException.ToString();
                    return response;
                }

                log.saveRequest("result.Content", result.Content);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    response.Status = Message.PROCESS;
                    APIError res = JsonConvert.DeserializeObject<APIError>(result.Content);
                    response.Response = $"{res?.httpCode}, {res?.httpMessage}, {res?.moreInformation}";
                    return response;
                }

                string descData = new EncryptDecrypt().Decrypt(Credential.MerchantKey, result.Content);
                log.saveRequest("Plain Response : ", descData);

                response.Response = descData;

                return response;
            }
            catch (Exception ex)
            {
                response.Status = Message.PROCESS;
                response.Response = ex.Message.ToString();
                log.saveRequest("Exception ", ex.Message);
            }

            return response;
        }

    }
}