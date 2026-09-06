using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YesBankCore.Controller
{
    internal class PayoutHTTPProcessing
    {
        private ErrorProcessing log = new ErrorProcessing();

        private RestRequest GetRequest()
        {
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("X-IBM-Client-Secret", PayoutCredential.XIBMClientSecret);
            request.AddHeader("X-IBM-Client-ID", PayoutCredential.XIBMClientID);

            string basicAuth = $"{PayoutCredential.AuthUserName}:{PayoutCredential.AuthPassword}";

            request.AddHeader("Authorization", $"Basic {Base64Encode(basicAuth)}");

            request.AddHeader("Content-Type", "application/json");

            //log.saveRequest("MerchantId", PayoutCredential.MerchantId);
            //log.saveRequest("MerchantKey", PayoutCredential.MerchantKey);
            //log.saveRequest("AuthUserName", PayoutCredential.AuthUserName);
            //log.saveRequest("AuthPassword", PayoutCredential.AuthPassword);
            //log.saveRequest("X-IBM-Client-Secret", PayoutCredential.XIBMClientSecret);
            //log.saveRequest("X-IBM-Client-ID", PayoutCredential.XIBMClientID);
            //log.saveRequest("Mode", PayoutCredential.Mode);
            //log.saveRequest("PrivateKeyPassword", PayoutCredential.PrivateKeyPassword);
            //log.saveRequest("PrivateKeyFilePath", PayoutCredential.PrivateKeyFilePath);

            return request;
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private void SetCertificate(RestClient client)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();

                string file = Path.Combine(Directory.GetCurrentDirectory(), PayoutCredential.PrivateKeyFilePath).Trim();
                byte[] data = File.ReadAllBytes(file);

                X509Certificate2 certificate = new X509Certificate2(data, PayoutCredential.PrivateKeyPassword,
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

        public NetworkResponse Post(string url, string JSONText, string orderID)
        {
            NetworkResponse response = new NetworkResponse();

            try
            {
                var client = new RestClient(url);
                client.ReadWriteTimeout = 5000 * 1000;
                SetCertificate(client);

                RestRequest request = GetRequest();

                log.saveRequest("URL", url);
                log.saveRequest("Payload", JSONText);

                request.AddParameter("application/json", JSONText, ParameterType.RequestBody);

                IRestResponse result = client.Execute(request);

                response.Status = Message.OK;

                log.saveRequest("result:" + orderID, result.Content);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    response.Status = Message.PROCESS;
                    APIError res = JsonConvert.DeserializeObject<APIError>(result.Content);
                    response.Response = $"{res?.httpCode}, {res?.httpMessage}, {res?.moreInformation}";
                    return response;
                }

                response.Response = result.Content;

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