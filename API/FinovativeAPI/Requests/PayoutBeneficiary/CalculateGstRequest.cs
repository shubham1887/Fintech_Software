using Project.Processing;
using Project.Requests.eKYC;

namespace Project.Requests.PayoutBeneficiary
{
    public class CalculateGstRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _Amount;
        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}
