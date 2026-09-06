using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesBankCore.Request.BBPS;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class Head1
    {
        private string _referenceId;
        public string referenceId
        {
            get => _referenceId.TrimIfNotNull();
            set => _referenceId = value.TrimIfNotNull();
        }
    }
    public class CustDetails2
    {
        private string _mobile;
        public string mobile
        {
            get => _mobile.TrimIfNotNull();
            set => _mobile = value.TrimIfNotNull();
        }
    }
    public class TxnList
    {
        private string _txnReferenceId;
        public string txnReferenceId
        {
            get => _txnReferenceId.TrimIfNotNull();
            set => _txnReferenceId = value.TrimIfNotNull();
        }

        private string _amount;
        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }
        private string _agentId;
        public string agentId
        {
            get => _agentId.TrimIfNotNull();
            set => _agentId = value.TrimIfNotNull();
        }

        private string _billerId;
        public string billerId
        {
            get => _billerId.TrimIfNotNull();
            set => _billerId = value.TrimIfNotNull();
        }
        private string _txnStatus;
        public string txnStatus
        {
            get => _txnStatus.TrimIfNotNull();
            set => _txnStatus = value.TrimIfNotNull();
        }

        public DateTime txnDate { get; set; }

    }
    public class TxnStatusComplainResp1
    {
        private string _msgId;
        public string msgId
        {
            get => _msgId.TrimIfNotNull();
            set => _msgId = value.TrimIfNotNull();
        }
        private string _complaintId;
        public string complaintId
        {
            get => _complaintId.TrimIfNotNull();
            set => _complaintId = value.TrimIfNotNull();
        }
        private string _openComplaint;
        public string openComplaint
        {
            get => _openComplaint.TrimIfNotNull();
            set => _openComplaint = value.TrimIfNotNull();
        }
        private string _complaintStatus;
        public string complaintStatus
        {
            get => _complaintStatus.TrimIfNotNull();
            set => _complaintStatus = value.TrimIfNotNull();
        }

        private string _assigned;
        public string assigned
        {
            get => _assigned.TrimIfNotNull();
            set => _assigned = value.TrimIfNotNull();
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

        public List<TxnList> txnList { get; set; }
        public CustDetails2 custDetails { get; set; }
    }
    public class Response3
    {
        public Head1 head { get; set; }
        public TxnStatusComplainResp1 txnStatusComplainResp { get; set; }
    }
    public class ComplaintStatusAPIResponse
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

        public Response3 response { get; set; }
    }
}
