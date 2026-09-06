using MplanCore.Controller;
using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class Record
    {
        private string _MonthlyRecharge;

        public string MonthlyRecharge
        {
            get => _MonthlyRecharge.TrimIfNotNull();
            set => _MonthlyRecharge = value.TrimIfNotNull();
        }

        private string _Balance;

        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }

        private string _customerName;

        public string customerName
        {
            get => _customerName.TrimIfNotNull();
            set => _customerName = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _NextRechargeDate;

        public string NextRechargeDate
        {
            get => _NextRechargeDate.TrimIfNotNull();
            set => _NextRechargeDate = value.TrimIfNotNull();
        }

        private string _planname;

        public string planname
        {
            get => _planname.TrimIfNotNull();
            set => _planname = value.TrimIfNotNull();
        }
    }

    public class SelectCustomerInfoResponse
    {
        private string _tel;

        public string tel
        {
            get => _tel.TrimIfNotNull();
            set => _tel = value.TrimIfNotNull();
        }

        private string _Operator;

        [JsonProperty("@operator")]
        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        public List<Record> records { get; set; }

        public SelectCustomerInfoResponse()
        {
            records = new List<Record>();
        }

        private int _status;

        public int status
        {
            get => _status;
            set => _status = value;
        }

        private string _desc;

        public string desc
        {
            get => _desc.TrimIfNotNull();
            set => _desc = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}