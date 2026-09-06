using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ComplaintResponse
{
    public class ComplaintsReportWrapper
    {
        private int _Count;
        private string _ReportPath;

        private bool _IsExcelDownload;

        public List<Project.Response.ComplaintResponse.ViewModel> list = new List<Project.Response.ComplaintResponse.ViewModel>();

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