using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.SignUp
{
    public class FailedUserWrapper
    {
        public List<SelectFailSignUpResponse> list = new List<SelectFailSignUpResponse>();

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