using MplanCore.Controller;

namespace MplanCore.Response
{
    public class ElectricityViewResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ConsumerNo;

        public string ConsumerNo
        {
            get => _ConsumerNo.TrimIfNotNull();
            set => _ConsumerNo = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _CustomerName;

        public string CustomerName
        {
            get => _CustomerName.TrimIfNotNull();
            set => _CustomerName = value.TrimIfNotNull();
        }

        private string _Billamount;

        public string Billamount
        {
            get => _Billamount.TrimIfNotNull();
            set => _Billamount = value.TrimIfNotNull();
        }

        private string _Billdate;

        public string Billdate
        {
            get => _Billdate.TrimIfNotNull();
            set => _Billdate = value.TrimIfNotNull();
        }

        private string _Duedate;

        public string Duedate
        {
            get => _Duedate.TrimIfNotNull();
            set => _Duedate = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _DueAmt;

        public string DueAmt
        {
            get => _DueAmt.TrimIfNotNull();
            set => _DueAmt = value.TrimIfNotNull();
        }
    }
}