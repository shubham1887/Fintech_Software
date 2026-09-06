using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionReport
{
    public class OldAmountAdjustmentReportWrapper
    {

        public List<Project.Response.UserAccounting.OldAmountAdjustmentViewModel> list = new List<Project.Response.UserAccounting.OldAmountAdjustmentViewModel>();
        public List<Project.Response.UserAccounting.NewAmountAdjustmentViewModel> NewAmountAdjustmentlist = new List<Project.Response.UserAccounting.NewAmountAdjustmentViewModel>();
       
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
