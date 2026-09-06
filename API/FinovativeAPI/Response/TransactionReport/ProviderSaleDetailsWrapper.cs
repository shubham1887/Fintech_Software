using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class ProviderSaleDetailsWrapper
    {
        public List<ProviderSaleViewModel> list = new List<ProviderSaleViewModel>();

        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }

      
        private bool _IsDownload;
        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }
    }
}
