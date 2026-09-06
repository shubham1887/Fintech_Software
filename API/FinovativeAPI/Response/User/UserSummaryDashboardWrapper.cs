using System.Collections.Generic;

namespace Project.Response.User
{
    public class UserSummaryDashboardWrapper
    {
        public List<WalletView> WalletList = new List<WalletView>();

        private string _UserType;

        public string UserType
        {
            get => _UserType;
            set => _UserType = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
    }
}