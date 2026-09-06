using Project.Processing;

namespace Project.Response.CashCollection

{
    public class Report
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName;
            set => _FirmName = value;
        }

        private string _Date;

        public string Date
        {
            get => _Date;
            set => _Date = value;
        }

        private float _OpeningBalance;

        public float OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private float _ClosingBalance;

        public float ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private float _Amount;

        public float Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private float _CollectionAmount;

        public float CollectionAmount
        {
            get => _CollectionAmount;
            set => _CollectionAmount = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark;
            set => _Remark = value;
        }

        private string _AssignedUserName;

        public string AssignedUserName
        {
            get => _AssignedUserName;
            set => _AssignedUserName = value;
        }

        public string Type { get; internal set; }

        private string _Credit;

        public string Credit
        {
            get => _Credit.TrimIfNotNull();
            set => _Credit = value.TrimIfNotNull();
        }

        private string _Debit;

        public string Debit
        {
            get => _Debit.TrimIfNotNull();
            set => _Debit = value.TrimIfNotNull();
        }

        public string AmountCreditDebit { get; internal set; }
        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }
    }
}