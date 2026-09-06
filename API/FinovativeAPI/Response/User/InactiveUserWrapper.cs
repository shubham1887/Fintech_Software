using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class InactiveUserWrapper
    {
        public List<SelectInActiveUserResponse> list = new List<SelectInActiveUserResponse>();

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

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
    }
}