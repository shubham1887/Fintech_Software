using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Operator
{
    public class OperatorWrapper
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