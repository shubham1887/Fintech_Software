using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class PendingChargeWrapper
    {
        public List<UserSubscriptionChargeViewModel> list = new List<UserSubscriptionChargeViewModel>();

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
