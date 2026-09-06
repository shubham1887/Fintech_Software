using Project.Processing;
using Project.Response.ReferralRegistration;
using System.Collections.Generic;

namespace Project.Requests.ReferralRegistration
{
    public class ReportWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();

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
        private bool _IsDownload;
        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }

    }
}