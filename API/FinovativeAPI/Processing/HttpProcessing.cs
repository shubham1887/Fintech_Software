using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Project.Processing
{
    public static class HttpProcessing
    {
        public static string DownloadedRazorPayQRImageName(string url, string orderID)
        {
            string imageName = $"Media/RazorPayQR/{orderID}.jpeg";
            string savepath = Path.Combine(Directory.GetCurrentDirectory(), imageName);

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), savepath);
            }

            return imageName;
        }

        private static string FillHeaderData(RestRequest request, string header)
        {
            string contentType = "application/json";
            if (header.Trim().Length <= 2)
            {
                return contentType;
            }

            try
            {
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
                if (dict != null && dict.Keys.Count > 0)
                {
                    foreach (var keyval in dict)
                    {
                        if (keyval.Key.EqualsIgnoreCase("Content-Type"))
                        {
                            contentType = keyval.Value;
                        }
                        request.AddHeader(keyval.Key, keyval.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return contentType;
        }

        private static void SetHTTPPS()
        {
            ServicePointManager.Expect100Continue = true;

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        public static string CallPostMethod(string baseURLWithBoday, string header, int timeoutInSecond = 120)
        {
            try
            {
                SetHTTPPS();

                Uri BalanceURI = new Uri(baseURLWithBoday);
                string baseUrlandPath = BalanceURI.Scheme + "://" + BalanceURI.Host + BalanceURI.AbsolutePath;
                string body = baseURLWithBoday.Replace(baseUrlandPath + "?", "");

                var client = new RestClient(baseUrlandPath)
                {
                    Timeout = timeoutInSecond * 1000,
                    ReadWriteTimeout = timeoutInSecond * 1000
                };
                client.Proxy = null;

                var request = new RestRequest(Method.POST);

                string contentType = FillHeaderData(request, header);

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter(contentType, body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.Content == null || response.Content.Trim().Length <= 2)
                {
                    new Logger().Info("HttpProcessing", "CallPostMethod", baseUrlandPath + ":-->:" + response.ErrorMessage?.ToString());
                    new Logger().Info("HttpProcessing", "CallPostMethod ErrorException", baseUrlandPath + ":-->:" + response.ErrorException?.ToString());
                }

                return response.Content;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return "";
        }

        // To call get method
        public static string CallGetMethod(string requestUrl, int timeoutInSecond = 120)
        {
            string result = string.Empty;
            try
            {
                if (requestUrl == null || requestUrl.Trim().Length <= 5)
                {
                    return "URL is blank or incorrect";
                }

                SetHTTPPS();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Timeout = timeoutInSecond * 1000;
                request.ReadWriteTimeout = timeoutInSecond * 1000;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy = null;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)";

                request.Headers["Connection"] = "keep-alive";
                request.Headers["Keep-Alive"] = Message.Utility.keepAliveTimeSpan;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream receiveStream = response.GetResponseStream())
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    result = readStream.ReadToEnd();
            }
            catch (IOException ex)
            {
                result = ex.Message;
                new Logger().Write(ex);
                new Logger().Info("HttpProcessing", "CallGetMethod exception", requestUrl);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                new Logger().Write(ex);
                new Logger().Info("HttpProcessing", "CallGetMethod exception", requestUrl);
            }
            return result;
        }

        internal static string Razorpay_PostMethod(string body, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient("https://api.razorpay.com/v1/orders");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddHeader("Authorization", "Basic " + encoded);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;

                //{"error":{"code":"BAD_REQUEST_ERROR","description":"The api key provided by you has expired and cannot be used. Please use correct key and secret.","source":"NA","step":"NA","reason":"NA","metadata":{}}}
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        internal static string Razorpay_StatusCheck(string paymentID, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                string URL = "https://api.razorpay.com/v1/payments/" + paymentID;
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddHeader("Authorization", "Basic " + encoded);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string Razorpay_OrderCheck(string OrderID, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                string URL = "https://api.razorpay.com/v1/orders/" + OrderID + "/payments";
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");

                request.AddHeader("Authorization", "Basic " + encoded);

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string Razorpay_CardDetails(string ID, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                string URL = "https://api.razorpay.com/v1/payments/" + ID + "/card";
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", "Basic " + encoded);

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        internal static string Paytm_PostMethod(string body, string URL)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }
        internal static string PayU_PostMethod(string Posturl, string Request)
        {
            string result = string.Empty;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new RestClient(Posturl);
                client.Timeout = -1;

                var request = new RestRequest(Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                JsonTextReader reader = new JsonTextReader(new StringReader(Request));
                while (reader.Read())
                {
                    var o = JObject.Load(reader);
                    foreach (var v in o)
                    {
                        var value = v.Value.Value<string>();
                        request.AddParameter(v.Key, value);
                    }
                }
               
                IRestResponse res = client.Execute(request);
                result = res.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string IMoneyPay_OrderCheck(string json)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();
                var client = new RestClient("https://prod.imoneypay.in/pgws/transact");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string PayGoal_OrderCheck(string json)
        {
            string result = string.Empty;
            string URL = "https://www.paygoal.in/paymentService/initiate";
            try
            {
                SetHTTPPS();
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }



        internal static string Cashfree_PostMethodPaymentGateway(string body, string AppID, string SecretKey)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient("https://api.cashfree.com/pg/orders");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-client-id", AppID);
                request.AddHeader("x-api-version", "2023-08-01");
                request.AddHeader("x-client-secret", SecretKey);
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal static string EaseBuzz_PostMethodForInitiatePayment(string body)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient("https://pay.easebuzz.in/payment/initiateLink");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
                new Logger().Info("HttpProcessing", "EaseBuzz InitiatePayment", result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        internal static string EaseBuzz_PostMethodForStatusCheck(string body)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient("https://dashboard.easebuzz.in/transaction/v1/retrieve");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;

                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
                new Logger().Info("HttpProcessing", "EaseBuzz StatusCheck", result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        internal static string UPIGateway_PostMethod(string URL, string body)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string GetBase64StringFromImageUrl(string url)
        {
            WebRequest req = WebRequest.Create(url);
            WebResponse resp = req.GetResponse();

            byte[] b = null;
            using (Stream stream = resp.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                int count = 0;
                do
                {
                    byte[] buf = new byte[1024];
                    count = stream.Read(buf, 0, 1024);
                    ms.Write(buf, 0, count);
                } while (stream.CanRead && count > 0);
                b = ms.ToArray();
            }
            return Encoding.ASCII.GetString(b);
        }

        public static string CredoPay_TransactionStatus(string orderID, string token)
        {
            string result = string.Empty;
            try
            {
                SetHTTPPS();

                var client = new RestClient($"https://aggregators.credopay.in:2083/aggregator/api/v1.0/transactions/status?custom_field1={orderID}");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", token);
                IRestResponse response = client.Execute(request);

                result = response.Content;

                new Logger().Info("HttpProcessing", "CredoPay_TransactionStatus", result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }
        public static string CredoPaySarata_TransactionStatus(string orderID, string token)
        {
            string result = string.Empty;
            try
            {
                SetHTTPPS();

                var client = new RestClient($"https://api.thecenterfirst.com/mms/v1/onboarding/transactions/status?custom_field1={orderID}");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", token);
                IRestResponse response = client.Execute(request);

                result = response.Content;

                new Logger().Info("HttpProcessing", "CredoPay_TransactionStatus", result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        internal static string RazorpayQR_PostMethod(string body, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient("https://api.razorpay.com/v1/payments/qr_codes");
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddHeader("Authorization", "Basic " + encoded);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string RazorpayQR_StatusCheck(string URLstr, string RazorpayKey, string RazorpaySecret)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                string URL = URLstr;
                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(RazorpayKey + ":" + RazorpaySecret));
                request.AddHeader("content-type", "application/json");

                request.AddHeader("Authorization", "Basic " + encoded);

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                IRestResponse response = client.Execute(request);
                result = response.Content;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string Worldline_PostMethod(string URL, string body)
        {
            string result = string.Empty;

            try
            {
                SetHTTPPS();

                var client = new RestClient(URL);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                result = response.Content;

                new Logger().Info("WorldLinePGCallbackController ", "Worldline_PostMethod", "result:" + result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public static string Payswiff_TransactionStatus(string body,string Mode)
        {
            string result = string.Empty;
            try
            {
                SetHTTPPS();
                 string url = UrlProvider.GetUrl(Mode);
                var client = new RestClient(url);
                client.Timeout = -1;
                client.ReadWriteTimeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                result = response.Content;

                new Logger().Info("HttpProcessing", "Payswiff_TransactionStatus", result);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return result;
        }

        public class UrlProvider
        {
            private const string UatUrl = "https://sandbox.payswiff.com/payments/v1.0/txnStatus";
            private const string LiveUrl = "https://secure.payswiff.com/payments/v1.0/txnStatus";

            public static string GetUrl(string requestType)
            {
                if (requestType.Equals("UAT", StringComparison.OrdinalIgnoreCase))
                {
                    return UatUrl;
                }

                return LiveUrl;
            }
        }

    }
}