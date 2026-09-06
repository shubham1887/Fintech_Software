using Projecdt.Response.TransactionReport;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class ProviderFailureWrapper
    {
        public List<ProviderFailureViewModel> list = new List<ProviderFailureViewModel>();

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