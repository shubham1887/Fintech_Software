using Project.Processing;

namespace Project.Response.Utility
{
    public class FetchViewModel
    {
        private bool _Status;

        public bool Status { get => _Status; set => _Status = value; }
        private string _CustomerName;

        public string CustomerName
        {
            get => _CustomerName.ToTitleCase();
            set => _CustomerName = value.ToTitleCase();
        }

        private string _DueDate;

        public string DueDate
        {
            get => _DueDate.TrimIfNotNull();
            set => _DueDate = value.TrimIfNotNull();
        }
        
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.TrimIfNotNull();
            set => _BillDate = value.TrimIfNotNull();
        }
        
        private string _BillerID;

        public string BillerID
        {
            get => _BillerID.TrimIfNotNull();
            set => _BillerID = value.TrimIfNotNull();
        }
        
        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _BBPSViewID;

        public string BBPSViewID
        {
            get => _BBPSViewID.TrimIfNotNull();
            set => _BBPSViewID = value.TrimIfNotNull();
        }

        private string _ResponseParam1;

        public string ResponseParam1
        {
            get => _ResponseParam1.TrimIfNotNull();

            set => _ResponseParam1 = value.TrimIfNotNull();
        }

        private string _ResponseParam2;

        public string ResponseParam2
        {
            get => _ResponseParam2.TrimIfNotNull();
            set => _ResponseParam2 = value.TrimIfNotNull();
        }

        private string _ResponseParam3;

        public string ResponseParam3
        {
            get => _ResponseParam3.TrimIfNotNull();
            set => _ResponseParam3 = value.TrimIfNotNull();
        }

        private string _ResponseParam4;

        public string ResponseParam4
        {
            get => _ResponseParam4.TrimIfNotNull();
            set => _ResponseParam4 = value.TrimIfNotNull();
        }

        private string _ResponseParam5;

        public string ResponseParam5
        {
            get => _ResponseParam5.TrimIfNotNull();
            set => _ResponseParam5 = value.TrimIfNotNull();
        }
    }
}