using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User

{
    public class UserWrapper
    {
        public List<SelectDatewiseResponse> list = new List<SelectDatewiseResponse>();

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
        private string _IsKYCApprovedStr;

        public string IsKYCApprovedStr
        {
            get => _IsKYCApprovedStr.TrimIfNotNull();
            set => _IsKYCApprovedStr = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        //public string IsKYCApprovedStr { get; set; }
    }
}