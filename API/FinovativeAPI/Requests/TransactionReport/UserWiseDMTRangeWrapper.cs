using Project.Processing;
using Project.Response.TransactionReport;
using System.Collections.Generic;

namespace project.Requests.TransactionReport
{
    public class UserWiseDMTRangeWrapper
    {

        public List<UserWiseDMTRangeViewModel> list = new List<UserWiseDMTRangeViewModel>();

       
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
