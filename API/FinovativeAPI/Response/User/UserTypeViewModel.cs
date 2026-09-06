using Project.Processing;

namespace Project.Response.User
{
    public class UserTypeViewModel
    {
        private int _userTypeID;
        private string _userType;

        public int UserTypeID
        {
            get => _userTypeID;
            set => _userTypeID = value;
        }

        public string UserType
        {
            get => _userType.TrimIfNotNull();
            set => _userType = value.TrimIfNotNull();
        }

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }

        private bool _IsOutside;

        public bool IsOutside
        {
            get => _IsOutside;
            set => _IsOutside = value;
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }
    }
}