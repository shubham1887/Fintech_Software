using System.Collections.Generic;

namespace Project.Response.WebHookSetting
{
    public class ReportWrapper
    {
        public List<RequestResponseViewModel> list = new List<RequestResponseViewModel>();

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
    }
}