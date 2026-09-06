using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.User
{
    public class GenerateOTPResponse
    {
        private string _Status;
       
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;
       
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private int _OTPMasterID;
      
        public int OTPMasterID
        {
            get => _OTPMasterID;
            set => _OTPMasterID = value;
        }
        private string _RedirectUrl;
        public string RedirectUrl
        {
            get => _RedirectUrl.TrimIfNotNull();
            set => _RedirectUrl = value.TrimIfNotNull();
        }

    }
}
