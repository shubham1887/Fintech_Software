using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.TransactionMaster
{
    public class AEPSReportWrapper
    {
        private int _Count;
        private string _ReportPath;

        

        public List<Project.Response.TransactionMaster.AEPSViewModel> list = new List<Project.Response.TransactionMaster.AEPSViewModel>();
        public List<Project.Response.TransactionMaster.AEPSExcelViewModel> Excellist = new List<Project.Response.TransactionMaster.AEPSExcelViewModel>();
        public List<Project.Response.TransactionMaster.MATMExcelViewModel> ExcellistMATM = new List<Project.Response.TransactionMaster.MATMExcelViewModel>();

        public int Count { get => _Count; set => _Count = value; }

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

        private bool _IsExcelDownload;
        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private HeaderViewModel headerSummary;

        public HeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }
    }
}