using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Slab
{
    public class SlabExcelWrapper
    {
        public List<DetailViewModel> list = new List<DetailViewModel>();

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