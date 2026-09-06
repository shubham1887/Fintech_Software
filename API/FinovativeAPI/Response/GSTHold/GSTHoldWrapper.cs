using System.Collections.Generic;

namespace Project.Response.GSTHold
{
    public class GSTHoldWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();

        private string _ReportPath;

        public string ReportPath { get => _ReportPath; set => _ReportPath = value; }
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _TotalCount; public int TotalCount { get => _TotalCount; set => _TotalCount = value; }
    }
}