using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesBankCore.Request.BBPS;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class CustDetails1
    {
        private string _mobile;
        public string mobile
        {
            get => _mobile.TrimIfNotNull();
            set => _mobile = value.TrimIfNotNull();
        }
    }
    public class TxnStatusComplainResp
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

        public List<object> txnList { get; set; }
        public CustDetails1 custDetails { get; set; }
    }

    public class Head
    {
        private string _referenceId;
        public string referenceId
        {
            get => _referenceId.TrimIfNotNull();
            set => _referenceId = value.TrimIfNotNull();
        }
    }
    public class Response2
    {
        public Head head { get; set; }
        public TxnStatusComplainResp txnStatusComplainResp { get; set; }
    }
    public class ComplaintAPIResponse
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

        public Response2 response { get; set; }
    }
}
