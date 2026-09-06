using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionMaster
{
    public class RechargeReportWrapper
    {
        public List<RechargeReportViewModel> list = new List<RechargeReportViewModel>();

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
        private bool _IsDownload;
        public bool IsDownload
        {

            get => _IsDownload;
            set => _IsDownload = value;
        }
    }
}