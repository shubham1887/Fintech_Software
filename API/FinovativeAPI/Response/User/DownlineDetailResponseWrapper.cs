using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class DownlineDetailResponseWrapper
    {
        public List<SelectDownlineReportResponse> data = new List<SelectDownlineReportResponse>();

        private BalanceHeaderViewModel headerSummary;

        public BalanceHeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }

        public List<Headerlist> headerlist = new List<Headerlist>();

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