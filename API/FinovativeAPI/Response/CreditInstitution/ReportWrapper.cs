using System.Collections.Generic;
using Project.Processing;

namespace Project.Response.CreditInstitution
{
    public class ReportWrapper
    {
        public List<AccountStatementViewModel> list = new List<AccountStatementViewModel>();

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
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
