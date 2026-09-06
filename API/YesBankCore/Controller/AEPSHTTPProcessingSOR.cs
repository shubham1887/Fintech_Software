using Newtonsoft.Json;
using RestSharp;
using System.Net;
using YesBankCore.Model;
using YesBankCore.Response.MoneyTransfer;
using YESBankCore.Controller;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YesBankCore.Controller
{
    internal class AEPSHTTPProcessingSOR
    {
        private ErrorProcessing log = new ErrorProcessing();

        private RestRequest GetRequest()
        {
            RestRequest request = new RestRequest(Method.POST);
            string encryptedPartnerKey = new EncryptDecrypt().Encryption(AEPSCredentials.PARTNERID, AEPSCredentials.PUBLICKEY);

            request.AddHeader("Auth", encryptedPartnerKey);

            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        public NetworkResponse PostForAEPS(string url, string requestStr)
        {
            NetworkResponse response = new NetworkResponse();
            try
            {
                var client = new RestClient(url);
                client.ReadWriteTimeout = 5000 * 1000;

                RestRequest request = GetRequest();

                log.saveRequest("URL", url);
                log.saveRequest("Plain Payload", requestStr);

                string eReq = new EncryptDecrypt().Encryption(requestStr, AEPSCredentials.PRIVATEKEY);

                log.saveRequest("Encrypt Payload", eReq);

                var requestBody = new { eReq = eReq };
                string requestJSON = JsonConvert.SerializeObject(requestBody);


                request.AddJsonBody(requestJSON);

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

                DMTAPIResponse APIResp = new DMTAPIResponse();
                APIResp = JsonConvert.DeserializeObject<DMTAPIResponse>(result.Content);

                if (APIResp == null)
                {
                    response.Status = Message.FAILURE;
                    response.Response = result.ErrorException.ToString();
                    return response;
                }
                string descData = new EncryptDecrypt().Decryption(APIResp.eResp, AEPSCredentials.PRIVATEKEY);

                log.saveRequest("Plain Response : ", descData);
                response.Response = descData;
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
