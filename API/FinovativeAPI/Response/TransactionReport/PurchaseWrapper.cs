using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class PurchaseWrapper
    {
        public List<PurchaseViewModel> list = new List<PurchaseViewModel>();

        private string _ReportPath;

        public string ReportPath { get => _ReportPath; set => _ReportPath = value; }
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _TotalCount; public int TotalCount { get => _TotalCount; set => _TotalCount = value; }
        public int SkipCount { get; set; }

        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
        }

        private string _SortColumnName;

        public string SortColumnName
        {
            get => _SortColumnName.TrimIfNotNull();
            set => _SortColumnName = value.TrimIfNotNull();
        }

        private string _SortDirection;

        public string SortDirection
        {
            get => _SortDirection.TrimIfNotNull();
            set => _SortDirection = value.TrimIfNotNull();
        }
    }
}