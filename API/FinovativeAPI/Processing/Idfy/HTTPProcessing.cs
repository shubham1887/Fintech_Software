using Project.StaticModel;
using Project.Response.SignUp;
using RestSharp;
using System;
using System.Net;

namespace Project.Processing.Idfy
{
    public class HTTPProcessing
    {
        public NetworkResponse POST(string Posturl, string Request, string AuthKey, string Token)
        {
            NetworkResponse netwotk = new NetworkResponse();
            netwotk.Status = StatusType.Failure;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var client = new RestClient(Posturl);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("api-key", AuthKey);
                request.AddHeader("account-id", Token);
                string postdata = Request.Trim();
                request.AddParameter("application/json", postdata, ParameterType.RequestBody);
                netwotk.Status = StatusType.Ok;
                IRestResponse response = client.Execute(request);

                if (response.Content == null || response.Content.Trim().Length <= 2)
                {
                    netwotk.Response = "Blank data";
                    return netwotk;
                }

                netwotk.Response = response.Content;
            }
            catch (Exception ex)
            {
                netwotk.Status = ex.Message;
            }

            return netwotk;
        }
    }
}
