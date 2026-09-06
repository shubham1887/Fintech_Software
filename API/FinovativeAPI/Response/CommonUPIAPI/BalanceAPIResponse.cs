using Project.Processing;

namespace Project.Response.CommonUPIAPI
{
    public class BalanceAPIResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private decimal _Balance;

        public decimal Balance
        {
            get => _Balance;
            set => _Balance = value;
        }
    }
}