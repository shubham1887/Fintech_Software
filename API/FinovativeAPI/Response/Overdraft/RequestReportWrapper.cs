using System.Collections.Generic;

namespace Project.Response.Overdraft
{
    public class RequestReportWrapper
    {
        public List<RequestViewModel> data = new List<RequestViewModel>();
        private int _Count;

        public int Count { get => _Count; set => _Count = value; }

        //private string _ReportPath;

        //private bool _IsExcelDownload;

        //public string ReportPath
        //{
        //    get => _ReportPath.TrimIfNotNull();
        //    set => _ReportPath = value.TrimIfNotNull();
        //}

        //public bool IsExcelDownload
        //{
        //    get => _IsExcelDownload;
        //    set => _IsExcelDownload = value;
        //}

        //private HeaderViewModel _HeaderViewModel;

        //public HeaderViewModel HeaderViewModel
        //{
        //    get => _HeaderViewModel;
        //    set => _HeaderViewModel = value;
        //}
    }
}