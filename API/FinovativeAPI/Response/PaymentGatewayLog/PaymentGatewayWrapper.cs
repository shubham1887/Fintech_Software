using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.PaymentGatewayLog
{
    public class PaymentGatewayWrapper
    {
        public List<PaymentGatewayLogViewModel> list = new List<PaymentGatewayLogViewModel>();
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