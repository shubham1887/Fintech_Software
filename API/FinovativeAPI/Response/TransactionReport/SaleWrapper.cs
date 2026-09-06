using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class SaleWrapper
    {
        public List<SaleViewModel> list = new List<SaleViewModel>();

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