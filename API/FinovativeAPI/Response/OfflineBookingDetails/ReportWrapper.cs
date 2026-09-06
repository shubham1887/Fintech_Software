using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.OfflineBookingDetails
{
    public class ReportWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();
        private Response.TransactionMaster.HeaderViewModel headerSummary;

        public Response.TransactionMaster.HeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
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