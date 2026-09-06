using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.VirtualBalance
{
    public class ReportWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();

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