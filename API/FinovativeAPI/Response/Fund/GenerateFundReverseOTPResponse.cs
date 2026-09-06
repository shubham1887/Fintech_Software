using Project.Processing;

namespace Project.Response.Fund
{
    public class GenerateFundReverseOTPResponse
    {
        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        public bool IsOTPSent { get; set; }
    }
}