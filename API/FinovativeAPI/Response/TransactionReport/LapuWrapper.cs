using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class LapuWrapper
    {
        public List<LapuViewModel> list = new List<LapuViewModel>();
        private Response.TransactionMaster.HeaderViewModel headerSummary;

        public Response.TransactionMaster.HeaderViewModel HeaderSummary
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