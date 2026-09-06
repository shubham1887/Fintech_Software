using System.Collections.Generic;

namespace Project.Response.News
{
    public class ReportWrapper
    {
        private int _Count;
        public int Count { get => _Count; set => _Count = value; }

        public List<ViewModel> list = new List<ViewModel>();
    }
}