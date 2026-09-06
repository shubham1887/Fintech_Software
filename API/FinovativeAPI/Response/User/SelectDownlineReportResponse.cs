using Newtonsoft.Json;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class SelectDownlineReportResponse
    {
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

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private decimal _RechargeBalance;

        public decimal RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private decimal _DMTBalance;

        public decimal DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private decimal _AEPSBalance;

        public decimal AEPSBalance
        {
            get => _AEPSBalance;
            set => _AEPSBalance = value;
        }

        private decimal _Outstanding;

        public decimal Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        public List<UserWallet> Wallets = new List<UserWallet>();

        public string Balance
        {
            get => JsonConvert.SerializeObject(Wallets);
        }

        private BalanceHeaderViewModel headerSummary;

        public BalanceHeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }
    }
}