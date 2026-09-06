using Project.Processing;

namespace Project.Response.User
{
    public class SelectDatewiseResponse
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _RegistrationType;

        public string RegistrationType
        {
            get => _RegistrationType.TrimIfNotNull();
            set => _RegistrationType = value.TrimIfNotNull();
        }

        private string _JoiningBy;

        public string JoiningBy
        {
            get => _JoiningBy.TrimIfNotNull();
            set => _JoiningBy = value.TrimIfNotNull();
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private string _Balances;

        public string Balances
        {
            get => _Balances.TrimIfNotNull();
            set => _Balances = value.TrimIfNotNull();
        }

        private bool _IsKYCApproved;

        public bool IsKYCApproved
        {
            get => _IsKYCApproved;
            set => _IsKYCApproved = value;
        }
        private bool _IsKYC;

        public bool IsKYC
        {
            get => _IsKYC;
            set => _IsKYC = value;
        }

        private string _IsKYCApprovedStr;

        public string IsKYCApprovedStr
        {
            get => _IsKYCApprovedStr.TrimIfNotNull();
            set => _IsKYCApprovedStr = value.TrimIfNotNull();
        }
    }
}