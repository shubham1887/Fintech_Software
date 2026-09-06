using System.Collections.Generic;

namespace Project.Response.Overdraft
{
    public class ReverseReportWrapper
    {
        public List<ReverseViewModel> data = new List<ReverseViewModel>();
        private int _Count;

        public int Count { get => _Count; set => _Count = value; }
    }
}