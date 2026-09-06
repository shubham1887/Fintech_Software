using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class AadharVerifyOtpResponse
    {
        private string _Status;

        [JsonProperty("Status")]
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        [JsonProperty("Message")]
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _FullName;

        [JsonProperty("FullName")]
        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }

        private string _Address;

        [JsonProperty("Address")]
        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _City;

        [JsonProperty("City")]
        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Gender;

        [JsonProperty("Gender")]
        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _Dob;

        [JsonProperty("Dob")]
        public string Dob
        {
            get => _Dob.TrimIfNotNull();
            set => _Dob = value.TrimIfNotNull();
        }

        private string _AadhaarNumber;

        [JsonProperty("AadhaarNumber")]
        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _Pin;

        [JsonProperty("Pin")]
        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _ProfileImage;

        [JsonProperty("ProfileImage")]
        public string ProfileImage
        {
            get => _ProfileImage.TrimIfNotNull();
            set => _ProfileImage = value.TrimIfNotNull();
        }

        private string _State;

        [JsonProperty("_State")]
        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }
    }
}