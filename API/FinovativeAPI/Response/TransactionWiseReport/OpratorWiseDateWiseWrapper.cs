
using Project.Processing;
using Project.Response.TransactionWiseReport;
using System.Collections.Generic;

namespace Project.Response.TransactionWiseReport
{
    public class OpratorWiseDateWiseWrapper
    {
        public List<OpratorWiseDateWiseViewModel> list = new List<OpratorWiseDateWiseViewModel>();


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
