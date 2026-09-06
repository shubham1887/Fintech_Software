using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class MoneyTransferAPIResponse
    {

        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _message;
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _customerMobile;
        public string customerMobile
        {
            get => _customerMobile.TrimIfNotNull();
            set => _customerMobile = value.TrimIfNotNull();
        }

        private string _rrn;
        public string rrn
        {
            get => _rrn.TrimIfNotNull();
            set => _rrn = value.TrimIfNotNull();
        }

        private double _amount;
        public double amount
        {
            get => _amount;
            set => _amount = value;
        }
        private string _beneficiaryName;
        public string beneficiaryName
        {
            get => _beneficiaryName.TrimIfNotNull();
            set => _beneficiaryName = value.TrimIfNotNull();
        }
       
    }
}
