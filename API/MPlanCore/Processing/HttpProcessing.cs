using MplanCore.Model;
using MplanCore.StaticModal;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text;

namespace MplanCore.Processing
{
    internal class HttpProcessing
    {
        private readonly ErrorProcessing log = new ErrorProcessing();

        // To call post method
        public static string CallPostMethod(string baseURL, string body, string header)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

                var client = new RestClient(baseURL)
                {
                    Timeout = 200 * 1000
                };

                var request = new RestRequest(Method.POST);

                if (dict != null && dict.Keys.Count > 0)
                {
                    foreach (var keyval in dict)
                        request.AddHeader(keyval.Key, keyval.Value);
                }

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                new ErrorProcessing().saveRequest("POST URL", baseURL);
                new ErrorProcessing().saveRequest("result", response.Content);

                return response.Content;
            }
            catch (Exception ex)
            {
                //  new Logger().Write(ex);
            }

            return "";
        }

        public NetworkResponse POST(string Posturl, string Request)
        {
            NetworkResponse netwotk = new NetworkResponse();
            netwotk.Status = Message.FAILURE;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(Posturl);
                client.Timeout = -1;
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                string postdata = Request.Trim();
                request.AddParameter("application/json", postdata, ParameterType.RequestBody);
                log.saveRequest("Posturl  : ", Posturl);
                log.saveRequest("Request  : ", postdata);
                netwotk.Status = Message.Ok;
                IRestResponse res = client.Execute(request);
                if (res.Content == null)
                {
                    netwotk.Response = "Blank data";
                    return netwotk;
                }
                if (res.Content.Length <= 0)
                {
                    netwotk.Response = "Blank data";
                    return netwotk;
                }
                log.saveRequest("Response  : ", res.Content);
                if (res.Content.ToLower().Contains("Ip is not whitelisted for the partner".ToLower()))
                {
                    netwotk.Status = Message.FAILURE;
                    netwotk.Response = "Ip is not whitelisted for the partner";
                    return netwotk;
                }
                if (res.Content.ToLower().Contains("You are not authorized to access this API".ToLower()))
                {
                    netwotk.Status = Message.FAILURE;
                    netwotk.Response = "You are not authorized to access this API";
                    return netwotk;
                }
                netwotk.Response = res.Content;
            }
            catch (Exception ex)
            {
                netwotk.Status = ex.Message;
            }
            return netwotk;
        }

        // To call get method
        public static string CallGetMethod(string requestUrl)
        {
            string result = string.Empty;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Timeout = 200 * 1000;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                result = readStream.ReadToEnd();
                response.Close();
                readStream.Close();

                new ErrorProcessing().saveRequest("GET URL", requestUrl);
                new ErrorProcessing().saveRequest("result", result);
            }
            catch (Exception ex)
            {
                //new Logger().Write(ex);
            }
            return result;
        }


        public static NetworkResponse CallGetMethodForMobiKwik(string Posturl, string Token = "14")
        {
            NetworkResponse netwotk = new NetworkResponse();
            netwotk.Status = StatusTypes.Failure;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;



                var client = new RestClient(Posturl);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-MClient", Token);

                netwotk.Status = StatusTypes.Ok;

                IRestResponse res = client.Execute(request);


                new ErrorProcessing().saveRequest(Posturl, res.Content);

                if (res.Content == null)
                {
                    netwotk.Response = "Blank data";
                    return netwotk;
                }

                if (res.Content.Length <= 0)
                {
                    netwotk.Response = "Blank data";
                    return netwotk;
                }

                netwotk.Response = res.Content;
            }
            catch (Exception ex)
            {
                netwotk.Status = ex.Message;
            }

            return netwotk;
        }

    }
}