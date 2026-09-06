using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class ComplaintResponse
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

        private string _ComplaintId;
        public string ComplaintId
        {
            get => _ComplaintId.TrimIfNotNull();
            set => _ComplaintId = value.TrimIfNotNull();
        }

        private string _ComplaintStatus;
        public string ComplaintStatus
        {
            get => _ComplaintStatus.TrimIfNotNull();
            set => _ComplaintStatus = value.TrimIfNotNull();
        }

        private string _OpenComplaint;
        public string OpenComplaint
        {
            get => _OpenComplaint.TrimIfNotNull();
            set => _OpenComplaint = value.TrimIfNotNull();
        }
    }
}
