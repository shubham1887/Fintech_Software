using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionMaster
{
    public class AccountOpeningReportWrapper
    {
        private int _Count;
        private string _ReportPath;

        private bool _IsExcelDownload;

        public List<Project.Response.TransactionMaster.ViewModel> list = new List<Project.Response.TransactionMaster.ViewModel>();

        public int Count { get => _Count; set => _Count = value; }

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private HeaderViewModel headerSummary;

        public HeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }
    }
}