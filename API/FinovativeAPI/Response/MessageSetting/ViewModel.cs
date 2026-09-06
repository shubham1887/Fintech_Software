using Project.Processing;

namespace Project.Response.MessageSetting
{
    public class ViewModel
    {
        private int _MessageSettingID;

        public int MessageSettingID
        {
            get => _MessageSettingID;
            set => _MessageSettingID = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private bool _IsSMS;

        public bool IsSMS
        {
            get => _IsSMS;
            set => _IsSMS = value;
        }

        private bool _ISOTP;

        public bool ISOTP
        {
            get => _ISOTP;
            set => _ISOTP = value;
        }

        private string _SmsMessage;

        public string SmsMessage
        {
            get => _SmsMessage.TrimIfNotNull();
            set => _SmsMessage = value.TrimIfNotNull();
        }

        private string _TemplateID;

        public string TemplateID
        {
            get => _TemplateID.TrimIfNotNull();
            set => _TemplateID = value.TrimIfNotNull();
        }

        private bool _IsWhatsUp;

        public bool IsWhatsUp
        {
            get => _IsWhatsUp;
            set => _IsWhatsUp = value;
        }

        private string _WhatsUpMessage;

        public string WhatsUpMessage
        {
            get => _WhatsUpMessage.TrimIfNotNull();
            set => _WhatsUpMessage = value.TrimIfNotNull();
        }

        private bool _IsMail;

        public bool IsMail
        {
            get => _IsMail;
            set => _IsMail = value;
        }

        private string _EmailMessage;

        public string EmailMessage
        {
            get => _EmailMessage.TrimIfNotNull();
            set => _EmailMessage = value.TrimIfNotNull();
        }
    }
}