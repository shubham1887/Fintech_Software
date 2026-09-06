using System.Collections.Generic;

namespace Project.Response.Fund
{
    public class RequestWrapper
    {
        public List<RequestViewModel> list = new List<RequestViewModel>();

        private RequestHeaderModel headerSummary;

        public RequestHeaderModel HeaderSummary
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
    }
}