using Project.Processing;
namespace Project.Response.MicroATM
{
    public class EzyPayCallbackResponse
    {
        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _Reatilerid;

        public string Reatilerid
        {
            get => _Reatilerid.TrimIfNotNull();
            set => _Reatilerid = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _RRN;

        public string RRN
        {
            get => _RRN.TrimIfNotNull();
            set => _RRN = value.TrimIfNotNull();
        }

        private string _TxnID;

        public string TxnID
        {
            get => _TxnID.TrimIfNotNull();
            set => _TxnID = value.TrimIfNotNull();
        }

        private string _Service;

        public string Service
        {
            get => _Service.TrimIfNotNull();
            set => _Service = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _TxnDate;

        public string TxnDate
        {
            get => _TxnDate.TrimIfNotNull();
            set => _TxnDate = value.TrimIfNotNull();
        }

        private string _AvailableBalance;

        public string AvailableBalance
        {
            get => _AvailableBalance.TrimIfNotNull();
            set => _AvailableBalance = value.TrimIfNotNull();
        }

        private string _CardNo;

        public string CardNo
        {
            get => _CardNo.TrimIfNotNull();
            set => _CardNo = value.TrimIfNotNull();
        }
    }
}
