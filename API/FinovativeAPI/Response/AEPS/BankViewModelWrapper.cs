using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.AEPS
{
    public class BankViewModelWrapper
    {
        private List<BankViewModel> _list;
        public List<BankViewModel> list { get => _list; set => _list = value; }

        private string _Message;

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}