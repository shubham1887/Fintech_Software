using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    public class AuthenticationResponse
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

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _DeviceType;

        public string DeviceType
        {
            get => _DeviceType.TrimIfNotNull();
            set => _DeviceType = value.TrimIfNotNull();
        }
        private string _DeviceModel;

        public string DeviceModel
        {
            get => _DeviceModel.TrimIfNotNull();
            set => _DeviceModel = value.TrimIfNotNull();
        }
    }
}
