using Project.Processing;

namespace Project.Response.ReconciliationSettings
{
    public class ViewModel
    {
        private int _ReconciliationSettingID;

        public int ReconciliationSettingID
        {
            get => _ReconciliationSettingID;
            set => _ReconciliationSettingID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _WorksheetName;

        public string WorksheetName
        {
            get => _WorksheetName.TrimIfNotNull();
            set => _WorksheetName = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _SuccessStatusAs;

        public string SuccessStatusAs
        {
            get => _SuccessStatusAs.TrimIfNotNull();
            set => _SuccessStatusAs = value.TrimIfNotNull();
        }

        private string _FailureStatusAs;

        public string FailureStatusAs
        {
            get => _FailureStatusAs.TrimIfNotNull();
            set => _FailureStatusAs = value.TrimIfNotNull();
        }
    }
}