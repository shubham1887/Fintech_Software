using Newtonsoft.Json;
using Project.BAL;
using Project.StaticModel;
using RestSharp;
using StackExchange.Profiling.Internal;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class FCMProcessing
    {
        private string fcmWebServerkey = "AAAAGmrylQ4:APA91bHtiVnccfp29VPkcvu2GIld6u90S8Gi6cLjaZQR8kx__Wqr1oLVCO0jUYcyS_8lGgP_j0dzdqZ8X_1TMZ1jxuWY4F5iYwPwPKctw7kcNTV7JmTdTbcvmi44fUqfSnjpJeP8qWyS";

        private string GetVarient(string status)
        {
            string varient;
            switch (status)
            {
                case "PROCESS":
                    varient = "warning";
                    break;

                case "SUCCESS":
                    varient = "success";
                    break;

                case "FAILURE":
                case "REFUND":

                    varient = "error";
                    break;

                default:
                    varient = "info";
                    break;
            }

            return varient;
        }

        public void SendNoticationMessage(string to, string message, string title = "", string status = "DEFAULT", int time = 10000, string defaultPlatform = PlatformCode.Web)
        {
            Task.Run(() => SendNoticationMessageCore(to, message, title, status, time, defaultPlatform));
        }

        private void SendNoticationMessageCore(string to, string message, string title = "", string status = "DEFAULT", int time = 10000, string defaultPlatform = PlatformCode.Web)
        {
            if (to == null || to.Length <= 4) return;

            string serverkey = "";

            if (defaultPlatform.EqualsIgnoreCase(PlatformCode.Web))
            {
                serverkey = fcmWebServerkey;
            }
            else
            {
                serverkey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FCM_SERVER_KEY));
            }

            if (serverkey.Length <= 10)
                return;

            string FCMSendURL = Messages.GoogleFCMSendURL;

            string varient = GetVarient(status);

            FCMNotificationMessageInput input = new FCMNotificationMessageInput
            {
                To = to,
                Sound = "default",
                Title = title.IsNullOrWhiteSpace() ? Company.ClientName : title,
                Body = message,
            };

            string logo = Company.WebAPIURL + Messages.DefaultLogoPath;

            input.Notification = new FCMMessage
            {
                Title = title.IsNullOrWhiteSpace() ? Company.ClientName : title,
                Body = message,
                Status = status,
                Param = "",
                AutoHideDuration = time,
                Variant = varient,
                Icon = logo,
            };

            string json = JsonConvert.SerializeObject(input, Formatting.None);

            var client = new RestClient(FCMSendURL);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            request.AddHeader("Authorization", "key=" + serverkey);

            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

            //if (defaultPlatform.EqualsIgnoreCase(PlatformCode.Web))
            //{
            //    request.AddHeader("Authorization", "key=" + serverkey);
            //}
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            try
            {
                var response = client.ExecuteAsync(request);

                //new Logger().Info("FCMProcessing", "Send input", json);
                //new Logger().Info("FCMProcessing", "Send", response.Result.Content.ToString());
                // Console.WriteLine("FCMProcessing Send" + response.Result.Content.ToString());
            }
            catch (Exception ex)
            {
                //new Logger().Write(ex);
            }
        }

        public void SendDataMessage(string to, string message, string title = "", string status = "DEFAULT", int time = 10000, string defaultPlatform = PlatformCode.Web)
        {
            Task.Run(() => SendDataMessageCore(to, message, title, status, time, defaultPlatform));
        }

        private void SendDataMessageCore(string to, string message, string title = "", string status = "DEFAULT", int time = 10000, string defaultPlatform = PlatformCode.Web)
        {
            if (to == null || to.Length <= 4) return;

            string serverkey = "";

            if (defaultPlatform.EqualsIgnoreCase(PlatformCode.Web))
            {
                serverkey = fcmWebServerkey;
            }
            else
            {
                serverkey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FCM_SERVER_KEY));
            }

            if (serverkey.Length <= 10)
                return;

            string FCMSendURL = Messages.GoogleFCMSendURL;

            string varient = GetVarient(status);

            FCMDataMessageInput input = new FCMDataMessageInput
            {
                To = to,
                Sound = "default",
                Title = title.IsNullOrWhiteSpace() ? Company.ClientName : title,
                Body = message,
            };

            string logo = Company.WebAPIURL + Messages.DefaultLogoPath;

            input.Data = new FCMMessage
            {
                Title = title.IsNullOrWhiteSpace() ? Company.ClientName : title,
                Body = message,
                Status = status,
                Param = "",
                AutoHideDuration = time,
                Variant = varient,
                Icon = logo,
            };

            string json = JsonConvert.SerializeObject(input, Formatting.None);

            var client = new RestClient(FCMSendURL);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            request.AddHeader("Authorization", "key=" + serverkey);

            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Keep-Alive", Message.Utility.keepAliveTimeSpan);

            //if (defaultPlatform.EqualsIgnoreCase(PlatformCode.Web))
            //{
            //    request.AddHeader("Authorization", "key=" + serverkey);
            //}
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            try
            {
                var response = client.ExecuteAsync(request);

                //Console.WriteLine("FCMProcessing Send" + response.Result.Content.ToString());
                //new Logger().Info("FCMProcessing", "Send input", json);
                //new Logger().Info("FCMProcessing", "Send", response.Result.Content.ToString());
            }
            catch (Exception ex)
            {
                //new Logger().Write(ex);
            }
        }
    }

    public class FCMNotificationMessageInput
    {
        [JsonProperty("notification")]
        public FCMMessage Notification { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("sound")]
        public string Sound { get; set; }

        public FCMNotificationMessageInput()
        {
            Notification = new FCMMessage();
        }
    }

    public class FCMDataMessageInput
    {
        [JsonProperty("data")]
        public FCMMessage Data { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("sound")]
        public string Sound { get; set; }

        public FCMDataMessageInput()
        {
            Data = new FCMMessage();
        }
    }

    public class FCMMessage
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("param")]
        public string Param { get; set; }

        [JsonProperty("autoHideDuration")]
        public int AutoHideDuration { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }

        [JsonProperty("priority")]
        public string Priority = "high";

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}