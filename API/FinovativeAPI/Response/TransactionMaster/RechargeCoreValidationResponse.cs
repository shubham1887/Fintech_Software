using Project.Model;
using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class RechargeCoreValidationResponse
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private decimal _CashBack;

        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }

        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        private bool _IsROfferRecharge;

        public bool IsROfferRecharge
        {
            get => _IsROfferRecharge;
            set => _IsROfferRecharge = value;
        }

        private int _InboxID;

        public int InboxID
        {
            get => _InboxID;
            set => _InboxID = value;
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        public CircleCore.Response.CoreResponse CircleResponse = new CircleCore.Response.CoreResponse();
        public LocationLookUp locationLookUp = new LocationLookUp();
        public Slab.DetailViewModel slabComm = new Slab.DetailViewModel();
    }
}