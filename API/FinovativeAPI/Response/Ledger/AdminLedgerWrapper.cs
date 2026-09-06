using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Ledger
{
    public class AdminLedgerWrapper
    {
        public List<AdminLedgerViewModel> data = new List<AdminLedgerViewModel>();
        private int _Count;

        public int Count { get => _Count; set => _Count = value; }

        private string _ReportPath;

        private bool _IsExcelDownload;

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