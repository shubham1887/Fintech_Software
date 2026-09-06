using Project.Processing;
using Project.Response.TransactionMaster;
using System.Collections.Generic;

namespace Project.Response.DematGrowthLossMaster
{
    public class ReportWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();

        private HeaderViewModel headerSummary;

        public HeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }
    }
}
