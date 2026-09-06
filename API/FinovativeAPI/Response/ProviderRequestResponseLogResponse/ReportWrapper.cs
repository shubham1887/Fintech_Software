using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ProviderRequestResponseLogResponse
{
    public class ReportWrapper
    {
        public List<RequestResponseViewModel> list = new List<RequestResponseViewModel>();

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

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