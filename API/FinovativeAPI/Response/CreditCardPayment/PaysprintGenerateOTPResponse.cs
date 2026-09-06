using Project.Processing;

namespace Project.Response.CreditCardPayment
{
    public class PaysprintGenerateOTPResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();

            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();

            set => _Message = value.TrimIfNotNull();
        }

        private string _OTPRefernce;

        public string OTPRefernce
        {
            get => _OTPRefernce.TrimIfNotNull();

            set => _OTPRefernce = value.TrimIfNotNull();
        }
    }
}