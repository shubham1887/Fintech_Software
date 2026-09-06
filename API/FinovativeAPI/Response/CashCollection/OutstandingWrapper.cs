using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.CashCollection
{
    public class OutstandingWrapper
    {

        public List<OutstandingViewModel> data = new List<OutstandingViewModel>();
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }
    }
}
