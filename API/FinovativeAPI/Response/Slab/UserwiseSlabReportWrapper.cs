using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Slab
{
    public class UserwiseSlabReportWrapper
    {
        public List<UserwiseSlabViewModel> list = new List<UserwiseSlabViewModel>();

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
    }
}