using Project.Processing;

namespace Project.Response.Operator
{
    public class ViewModelForDropDown
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _Path;

        public string Path
        {
            get => _Path.TrimIfNotNull();
            set => _Path = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsBBPS;
        public bool IsBBPS { get => _IsBBPS; set => _IsBBPS = value; }

        private string _AccountName;

        public string AccountName
        {
            get { return _AccountName.TrimIfNotNull(); }
            set { _AccountName = value.TrimIfNotNull(); }
        }

        private string _Remark;

        public string Remark
        {
            get { return _Remark.TrimIfNotNull(); }
            set { _Remark = value.TrimIfNotNull(); }
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private bool _IsPartialBill;

        public bool IsPartialBill
        {
            get { return _IsPartialBill; }
            set { _IsPartialBill = value; }
        }

        private decimal _MinAmount;
        public decimal MinAmount { get => _MinAmount; set => _MinAmount = value; }

        private decimal _MaxAmount;
        public decimal MaxAmount { get => _MaxAmount; set => _MaxAmount = value; }

        private bool _IsBillFetch;
        public bool IsBillFetch { get => _IsBillFetch; set => _IsBillFetch = value; }
        private string _AllowedFor;

        public string AllowedFor
        {
            get => _AllowedFor.TrimIfNotNull();
            set => _AllowedFor = value.TrimIfNotNull();
        }
    }
}