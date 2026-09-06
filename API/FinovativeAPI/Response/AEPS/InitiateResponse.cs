using Project.Processing;

namespace Project.Response.AEPS
{
    public class InitiateResponse
    {
        private bool _IsServiceEnabled;

        public bool IsServiceEnabled
        {
            get => _IsServiceEnabled;
            set => _IsServiceEnabled = value;
        }

        private bool _IsChargeDeducted;

        public bool IsChargeDeducted
        {
            get => _IsChargeDeducted;
            set => _IsChargeDeducted = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _UniqueRefNo;

        public string UniqueRefNo
        {
            get => _UniqueRefNo.TrimIfNotNull();
            set => _UniqueRefNo = value.TrimIfNotNull();
        }

        private string _RequestId;

        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }

        private string _UDD;

        public string UDD
        {
            get => _UDD.TrimIfNotNull();
            set => _UDD = value.TrimIfNotNull();
        }

        private string _DeveloperKey;

        public string DeveloperKey
        {
            get => _DeveloperKey.TrimIfNotNull();
            set => _DeveloperKey = value.TrimIfNotNull();
        }

        private string _DeveloperPass;

        public string DeveloperPass
        {
            get => _DeveloperPass.TrimIfNotNull();
            set => _DeveloperPass = value.TrimIfNotNull();
        }

        private string _UserMobileNumber;

        public string UserMobileNumber
        {
            get => _UserMobileNumber.TrimIfNotNull();
            set => _UserMobileNumber = value.TrimIfNotNull();
        }
    }
}