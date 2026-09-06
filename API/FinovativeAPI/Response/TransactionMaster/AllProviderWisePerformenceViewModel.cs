using Project.Processing;
using System;

namespace project.Response.TransactionMaster
{
    public class AllProviderWisePerformenceViewModel
    {
        private double _ProcTime;

        public double ProcTime
        {
            get => _ProcTime;
            set => _ProcTime = value;
        }
        private string _IntervalStart;

        public string IntervalStart
        {
            get => _IntervalStart;
            set => _IntervalStart = value.TrimIfNotNull();
        }
        private string _IntervalEnd;

        public string IntervalEnd
        {
            get => _IntervalEnd;
            set => _IntervalEnd = value.TrimIfNotNull();
        }
        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName;
            set => _ProviderName = value.TrimIfNotNull();
        }
        private double _AvgProcTime;

        public double AvgProcTime
        {
            get => _AvgProcTime;
            set => _AvgProcTime = value;
        }

        private DateTime _DOC;

        public DateTime DOC
        {
            get => _DOC;
            set => _DOC = value;
        }
    
    }
}

