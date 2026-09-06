using Project.Processing;

namespace Project.Response.User
{
    public class SelectInActiveUserResponse
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _ParentName;

        public string ParentName
        {
            get => _ParentName.TrimIfNotNull();
            set => _ParentName = value.TrimIfNotNull();
        }

        private string _Balance;

        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }

        private string _LastTransactionDate;

        public string LastTransactionDate
        {
            get => _LastTransactionDate.TrimIfNotNull();
            set => _LastTransactionDate = value.TrimIfNotNull();
        }

        private string _UserType;
        public string UserType { get => _UserType; set => _UserType = value; }

        private int _Count;
        public int Count { get => _Count; set => _Count = value; }
    }
}