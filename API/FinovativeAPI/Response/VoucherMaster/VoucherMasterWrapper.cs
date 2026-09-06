using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.VoucherMaster
{
    public class VoucherMasterWrapper
    {
        public List<Project.Response.VoucherMaster.ViewModel> list = new List<Project.Response.VoucherMaster.ViewModel>();
        public Project.Response.VoucherMaster.VoucherSummaryViewModel VoucherSummary = new Project.Response.VoucherMaster.VoucherSummaryViewModel();

    
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
