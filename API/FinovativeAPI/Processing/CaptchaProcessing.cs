using Newtonsoft.Json;
using Project.DAL;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class CaptchaProcessing
    {
        private static string URL;
        private static string Secret;

        private void SetCaptchaCredentials()
        {
            URL = "https://www.google.com/recaptcha/api/siteverify";

            Secret = new SettingMasterDal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.GOOGLE_CAPTCHA_SECRET_KEY));
        }

        public async Task<bool> IsValidCaptcha(string captchaToken, string iPAddress, string device)
        {
            if (!device.EqualsIgnoreCase(StaticModel.PlatformCode.Web))
            {
                return true;
            }

            await Task.Delay(1);

            string IsGoogleCaptcha = new SettingMasterDal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsGoogleCaptcha));

            if (IsGoogleCaptcha.EqualsIgnoreCase("No"))
            {
                return true;
            }

            SetCaptchaCredentials();

            if (Secret.Length <= 5)
            {
                return true;
            }

            URL = $"{URL}?secret={Secret}&response={captchaToken}&remoteip={iPAddress}";
            string response = HttpProcessing.CallGetMethod(URL, 30);

            if (response == null)
            {
                return true;
            }

            VerifyResponse responseModel = new VerifyResponse();
            try
            {
                responseModel = JsonConvert.DeserializeObject<VerifyResponse>(response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            await Task.Delay(1);

            if (responseModel == null) return false;

            if (!responseModel.Success) return false;
            return true;
        }
    }

    internal class VerifyResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public string ChallengeTs { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("error-codes")]
        public string ErrorCodes { get; set; }
    }
}