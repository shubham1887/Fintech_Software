using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class Response1
    {
        private int _chId;
        public int chId
        {
            get => _chId;
            set => _chId = value;
        }

        private string _refId;
        public string refId
        {
            get => _refId.TrimIfNotNull();
            set => _refId = value.TrimIfNotNull();
        }

        private string _approvalRefNum;
        public string approvalRefNum
        {
            get => _approvalRefNum.TrimIfNotNull();
            set => _approvalRefNum = value.TrimIfNotNull();
        }

        private string _responseCode;
        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

        private string _responseReason;
        public string responseReason
        {
            get => _responseReason.TrimIfNotNull();
            set => _responseReason = value.TrimIfNotNull();
        }

        private string _complianceReason;
        public string complianceReason
        {
            get => _complianceReason.TrimIfNotNull();
            set => _complianceReason = value.TrimIfNotNull();
        }

        private string _txnDateTime;
        public string txnDateTime
        {
            get => _txnDateTime.TrimIfNotNull();
            set => _txnDateTime = value.TrimIfNotNull();
        }

        private string _txnReferenceId;
        public string txnReferenceId
        {
            get => _txnReferenceId.TrimIfNotNull();
            set => _txnReferenceId = value.TrimIfNotNull();
        }
    }
    public class BillPayAPIResponse
    {
        private string _respCode;
        public string respCode
        {
            get => _respCode.TrimIfNotNull();
            set => _respCode = value.TrimIfNotNull();
        }
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        public Response1 response { get; set; }
    }
}
