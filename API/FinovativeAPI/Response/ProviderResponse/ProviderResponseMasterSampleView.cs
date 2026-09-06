using Project.Processing;

namespace Project.Response.ProviderResponse
{
    public class ProviderResponseMasterSampleView
    {
        private int _ProviderResponseMasterSampleID;

        public int ProviderResponseMasterSampleID
        {
            get => _ProviderResponseMasterSampleID;
            set => _ProviderResponseMasterSampleID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private string _TextMustexits;

        public string TextMustexits
        {
            get => _TextMustexits.TrimIfNotNull();
            set => _TextMustexits = value.TrimIfNotNull();
        }

        private string _Provider;

        public string Provider
        {
            get => _Provider.TrimIfNotNull();
            set => _Provider = value.TrimIfNotNull();
        }
    }
}