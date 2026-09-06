using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionWiseReport
{
    public class UserWiseOpratorWiseWrapper
    {
        public List<UserWiseOpratorWiseViewModel> list = new List<UserWiseOpratorWiseViewModel>();


        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private bool _IsDownload;
        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }
    }
}
