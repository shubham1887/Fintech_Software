using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PushNotifications;
using FintechApp.StaticModels;
using Newtonsoft.Json;
using System.Text;

namespace FintechApp.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IUserRepository _userRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public PushNotificationService(IUserRepository userRepo, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = new Logger();
        }

        public async Task<string> SendToUserTypes(SendPushNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title)) return "Title is required.";
            if (string.IsNullOrWhiteSpace(request.Description)) return "Description is required.";
            if (request.UserTypeList == null || request.UserTypeList.Count == 0) return "User type list is required.";

            string appServerKey = _configuration["FCM:ServerKey"] ?? string.Empty;
            string webServerKey = _configuration["FCM:WebServerKey"] ?? string.Empty;
            string fcmUrl = _configuration["FCM:SendUrl"] ?? "https://fcm.googleapis.com/fcm/send";

            var distinctTypeIds = request.UserTypeList
                .Select(x => x.UserTypeID)
                .Distinct()
                .ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    foreach (int userTypeId in distinctTypeIds)
                    {
                        var users = await _userRepo.GetUsersByUserType(userTypeId);
                        foreach (var user in users)
                        {
                            if (!string.IsNullOrWhiteSpace(user.AppFCMToken) && !string.IsNullOrWhiteSpace(appServerKey))
                                _ = SendFcmAsync(user.AppFCMToken, request.Title, request.Description, appServerKey, fcmUrl);

                            if (!string.IsNullOrWhiteSpace(user.WebFCMToken) && !string.IsNullOrWhiteSpace(webServerKey))
                                _ = SendFcmAsync(user.WebFCMToken, request.Title, request.Description, webServerKey, fcmUrl);
                        }
                    }
                }
                catch (Exception ex) { _logger.Write(ex); }
            });

            return StatusType.Ok;
        }

        private async Task SendFcmAsync(string token, string title, string body, string serverKey, string fcmUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || token.Length <= 4) return;

                var payload = new
                {
                    to = token,
                    notification = new
                    {
                        title,
                        body,
                        sound = "default"
                    }
                };

                string json = JsonConvert.SerializeObject(payload);
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + serverKey);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(fcmUrl, content);
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
