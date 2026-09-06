using Project.Processing;

namespace Project.Response.Provider
{
    public class ViewModel
    {
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

        private decimal _CurrentBalance;

        public decimal CurrentBalance
        {
            get => _CurrentBalance;
            set => _CurrentBalance = value;
        }

        private decimal _RechargeBalance;

        public decimal RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private decimal _DMTBalance;

        public decimal DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private string _ProviderUrl;

        public string ProviderUrl
        {
            get => _ProviderUrl.TrimIfNotNull();
            set => _ProviderUrl = value.TrimIfNotNull();
        }

        private string _ProviderUniqueID;

        public string ProviderUniqueID
        {
            get => _ProviderUniqueID.TrimIfNotNull();
            set => _ProviderUniqueID = value.TrimIfNotNull();
        }

        private int _ResponseGroupID;

        public int ResponseGroupID
        {
            get => _ResponseGroupID;
            set => _ResponseGroupID = value;
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public bool IsHLR
        {
            get => _IsHLR;
            set => _IsHLR = value;
        }

        private bool _IsHLR;

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        private int _CopyFromProviderID;

        public int CopyFromProviderID
        {
            get { return _CopyFromProviderID; }
            set { _CopyFromProviderID = value; }
        }

        private bool _IsRealAPI;

        public bool IsRealAPI
        {
            get => _IsRealAPI;
            set => _IsRealAPI = value;
        }

        private string _BalanceDetails;

        public string BalanceDetails
        {
            get => _BalanceDetails;
            set => _BalanceDetails = value;
        }

        private string _LoginID;

        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _SupportNumber;

        public string SupportNumber
        {
            get => _SupportNumber.TrimIfNotNull();
            set => _SupportNumber = value.TrimIfNotNull();
        }

        private int _LowBalanceAmount;

        public int LowBalanceAmount
        {
            get => _LowBalanceAmount;
            set => _LowBalanceAmount = value;
        }
    }
}