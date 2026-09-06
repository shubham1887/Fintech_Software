using Project.Processing;

namespace Project.Response.VirtualBalance
{
    public class AddResponse
    {
        public decimal Amount;
        public bool IsOTPSent;

        public int WalletID { get; internal set; }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}