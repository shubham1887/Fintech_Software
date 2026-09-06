using System;

namespace Project.Response.TransactionMaster
{
    public class Last12hrViewModel
    {


        public double ProcTime { get; internal set; }
        public string IntervalStart { get; set; }
        public string IntervalEnd { get; set; }
        public double AvgProcTime { get; internal set; }
        public DateTime DOC { get; internal set; }
    }
}
