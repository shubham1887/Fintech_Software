using Project.Processing;
using Project.Requests.TransactionReport;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class UserSaleWrapper
    {
        public List<UserSaleViewModel> list = new List<UserSaleViewModel>();

         private HeaderUserSaleViewModel headerSummary;

        public HeaderUserSaleViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }

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