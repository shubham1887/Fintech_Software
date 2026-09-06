using Project.Processing;

namespace Project.Response.User
{
    public class DeletedUsersResponse
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _ParentName;

        public string ParentName
        {
            get => _ParentName.TrimIfNotNull();
            set => _ParentName = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private float _RechargeBalance;

        public float RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private float _DMTBalance;

        public float DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private float _AEPSBalance;

        public float AEPSBalance
        {
            get => _AEPSBalance;
            set => _AEPSBalance = value;
        }

        private float _Outstanding;

        public float Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private string _Balances;

        public string Balances
        {
            get => _Balances;
            set => _Balances = value;
        }
    }
}