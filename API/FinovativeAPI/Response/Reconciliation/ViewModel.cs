using Project.Processing;

namespace Project.Response.Reconciliation
{
    public class ViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Number;

        public string Number
        {
            get => _Number.TrimIfNotNull();
            set => _Number = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _DBStatus;

        public string DBStatus
        {
            get => _DBStatus.TrimIfNotNull();
            set => _DBStatus = value.TrimIfNotNull();
        }

        private string _ExcelStatus;

        public string ExcelStatus
        {
            get => _ExcelStatus.TrimIfNotNull();
            set => _ExcelStatus = value.TrimIfNotNull();
        }

        private bool _IsMismached;

        public bool IsMismached
        {
            get => _IsMismached;
            set => _IsMismached = value;
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
    }
}