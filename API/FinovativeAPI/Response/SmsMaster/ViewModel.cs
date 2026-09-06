using Project.Processing;

namespace Project.Response.SmsMaster
{
    public class ViewModel
    {
        private int _SMSMasterID;

        public int SMSMasterID
        {
            get => _SMSMasterID;
            set => _SMSMasterID = value;
        }

        private string _SMSString;

        public string SMSString
        {
            get => _SMSString.TrimIfNotNull();
            set => _SMSString = value.TrimIfNotNull();
        }

        private string _SMSOTPURL;

        public string SMSOTPURL
        {
            get => _SMSOTPURL.TrimIfNotNull();
            set => _SMSOTPURL = value.TrimIfNotNull();
        }

        private string _BalanceURL;

        public string BalanceURL
        {
            get => _BalanceURL.TrimIfNotNull();
            set => _BalanceURL = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _APIName;

        public string APIName
        {
            get => _APIName.TrimIfNotNull();
            set => _APIName = value.TrimIfNotNull();
        }

        private string _WhatsAppURL;

        public string WhatsAppURL
        {
            get => _WhatsAppURL.TrimIfNotNull();
            set => _WhatsAppURL = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _MobileWithOrWithout91;

        public string MobileWithOrWithout91
        {
            get => _MobileWithOrWithout91.TrimIfNotNull();
            set => _MobileWithOrWithout91 = value.TrimIfNotNull();
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _MethodTypeWhatsApp;

        public string MethodTypeWhatsApp
        {
            get => _MethodTypeWhatsApp.TrimIfNotNull();
            set => _MethodTypeWhatsApp = value.TrimIfNotNull();
        }

        private string _HeaderJsonWhatsApp;

        public string HeaderJsonWhatsApp
        {
            get => _HeaderJsonWhatsApp.TrimIfNotNull();
            set => _HeaderJsonWhatsApp = value.TrimIfNotNull();
        }
    }
}