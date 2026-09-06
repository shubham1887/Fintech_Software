using MplanCore.Controller;

namespace Project.Response.CashCollection
{
    public class SelectByUserIDResponse
    {
        private string _BalanceDetail;

        public string BalanceDetail
        {
            get => _BalanceDetail.TrimIfNotNull();
            set => _BalanceDetail = value.TrimIfNotNull();
        }

        private decimal _Outstanding;

        public decimal Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private decimal _MaxOutStanding;

        public decimal MaxOutStanding
        {
            get => _MaxOutStanding;
            set => _MaxOutStanding = value;
        }
    }
}