namespace Project.Response
{
    public class ProviderOperatorView
    {
        private int _ProviderOperatorID;
        private int _ProviderID;
        private int _ServiceID;
        private string _Operatorcode;
        private string _MarginType;
        private decimal _CommissionAdmin;
        private int _PendingCount;
        private int _FailureCount;
        private bool _IsActive;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        public string Operatorcode
        {
            get => _Operatorcode;
            set => _Operatorcode = value;
        }

        public string MarginType
        {
            get => _MarginType;
            set => _MarginType = value;
        }

        public decimal CommissionAdmin
        {
            get => _CommissionAdmin;
            set => _CommissionAdmin = value;
        }

        public int PendingCount
        {
            get => _PendingCount;
            set => _PendingCount = value;
        }

        public int FailureCount
        {
            get => _FailureCount;
            set => _FailureCount = value;
        }

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public int ProviderOperatorID
        {
            get => _ProviderOperatorID;
            set => _ProviderOperatorID = value;
        }

        public string ServiceName { get; internal set; }
        public string OperatorName { get; internal set; }
    }
}