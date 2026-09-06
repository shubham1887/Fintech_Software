using Project.Processing;

namespace Project.Response.ProviderResponse
{
    public class ViewModel
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _TextMustExists;

        public string TextMustExists
        {
            get => _TextMustExists.TrimIfNotNull();
            set => _TextMustExists = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public bool IsRefCode
        {
            get => _IsRefCode;
            set => _IsRefCode = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _ProviderResponseMasterID;

        public int ProviderResponseMasterID
        {
            get => _ProviderResponseMasterID;
            set => _ProviderResponseMasterID = value;
        }

        public string Status { get; internal set; }

        private bool _IsActive;

        private bool _IsRefCode;
    }
}