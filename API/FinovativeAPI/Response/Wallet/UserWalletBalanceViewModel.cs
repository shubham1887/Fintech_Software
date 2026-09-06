using Project.Processing;
using System;

namespace Project.Response.Wallet
{
    public class UserWalletBalanceViewModel
    {
        private String _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private float _ClosingBalance;

        public float ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private float _OpeningBalance;

        public float OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private int _WalletID;
        public int WalletID { get => _WalletID; set => _WalletID = value; }
    }
}