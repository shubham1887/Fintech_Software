using Project.Processing;

namespace Project.Response.Beneficiary
{
    public class CheckBranchResponse
    {
        private string _Branch;

        public string Branch
        {
            get => _Branch.TrimIfNotNull();
            set => _Branch = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNullWithDefault("Dummy");
            set => _IFSC = value.TrimIfNotNullWithDefault("Dummy");
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}