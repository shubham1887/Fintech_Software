using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class RangewiseRechargeModel
    {
        private string _Range;

        public string Range
        {
            get => _Range.TrimIfNotNull();
            set => _Range = value.TrimIfNotNull();
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private decimal _Sum;

        public decimal Sum
        {
            get => _Sum;
            set => _Sum = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _SuccessSum;

        public decimal SuccessSum
        {
            get => _SuccessSum;
            set => _SuccessSum = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }

        private decimal _FailSum;

        public decimal FailSum
        {
            get => _FailSum;
            set => _FailSum = value;
        }

        private int _ProcessCount;

        public int ProcessCount
        {
            get => _ProcessCount;
            set => _ProcessCount = value;
        }

        private decimal _ProcessSum;

        public decimal ProcessSum
        {
            get => _ProcessSum;
            set => _ProcessSum = value;
        }

        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }
    }
}