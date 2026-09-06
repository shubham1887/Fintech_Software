namespace Project.Response.TransactionReport
{
    public class RofferViewModel
    {
        private decimal _RofferComm;

        public decimal RofferComm
        {
            get => _RofferComm;
            set => _RofferComm = value;
        }

        private decimal _RofferTotal;

        public decimal RofferTotal
        {
            get => _RofferTotal;
            set => _RofferTotal = value;
        }

        private string _RofferPer;

        public string RofferPer
        {
            get => _RofferPer;
            set => _RofferPer = value;
        }
    }
}