using Project.Processing;
using Project.Response.Wallet;
using System.Collections.Generic;

namespace Project.Response
{
    public class UserBackup
    {
        public List<DetailViewModel> list { get; set; }

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

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private float _Outstanding;

        public float Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private string _ParentFirmName;

        public string ParentFirmName
        {
            get => _ParentFirmName.TrimIfNotNull();
            set => _ParentFirmName = value.TrimIfNotNull();
        }

        private string _ParentMobileNumber;

        public string ParentMobileNumber
        {
            get => _ParentMobileNumber.TrimIfNotNull();
            set => _ParentMobileNumber = value.TrimIfNotNull();
        }

        private float rechargeBalance;

        public float RechargeBalance
        {
            get => rechargeBalance;
            set => rechargeBalance = value;
        }

        private float dMTBalance;

        public float DMTBalance
        {
            get => dMTBalance;
            set => dMTBalance = value;
        }

        private float aEPSBalance;

        public float AEPSBalance
        {
            get => aEPSBalance;
            set => aEPSBalance = value;
        }

        private string _WalletString;

        public string WalletString
        {
            get => _WalletString.TrimIfNotNull();
            set => _WalletString = value.TrimIfNotNull();
        }
    }
}