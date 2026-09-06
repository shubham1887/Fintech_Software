using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.eKYC
{
    public class eKycReportWrapper
    {
        private int _Count;
        private string _ReportPath;

        private bool _IsExcelDownload;

        public List<EkycDataViewModel> list = new List<EkycDataViewModel>();


        public int Count { get => _Count; set => _Count = value; }

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }
    }
}