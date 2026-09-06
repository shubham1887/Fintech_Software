using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class ComplaintStatusResponse
    {

        private string _Status;
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        private string _Message;
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}
