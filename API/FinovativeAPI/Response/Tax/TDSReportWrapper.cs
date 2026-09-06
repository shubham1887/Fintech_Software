using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Tax
{
    public class TDSReportWrapper
    {
        public List<TDSReportViewModel> list = new List<TDSReportViewModel>();

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

       
        private bool _IsDownload;
        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }

    }
}