using FintechApp.Helpers;

namespace FintechApp.Responses.Wallet
{
    public class BackupMailWalletViewModel
    {
        public int WalletID { get; set; }
        public int UserID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private float _Balance;

        public float Balance
        {
            get => _Balance;
            set => _Balance = value;
        }

        private string _ShortName;

        public string ShortName
        {
            get => _ShortName.TrimIfNotNull();
            set => _ShortName = value.TrimIfNotNull();
        }


    }
}
