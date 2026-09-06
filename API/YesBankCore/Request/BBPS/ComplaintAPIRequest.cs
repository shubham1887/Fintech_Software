using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class ComplainReq
    {

        private string _agentId;
        public string agentId
        {
            get => _agentId.TrimIfNotNull();
            set => _agentId = value.TrimIfNotNull();
        }
        
        private string _complaintType;
        public string complaintType
        {
            get => _complaintType.TrimIfNotNull();
            set => _complaintType = value.TrimIfNotNull();
        }
        
        private string _mobile;
        public string mobile
        {
            get => _mobile.TrimIfNotNull();
            set => _mobile = value.TrimIfNotNull();
        }
        
        private string _description;
        public string description
        {
            get => _description.TrimIfNotNull();
            set => _description = value.TrimIfNotNull();
        }
        
        private string _disposition;
        public string disposition
        {
            get => _disposition.TrimIfNotNull();
            set => _disposition = value.TrimIfNotNull();
        }
        
        private string _txnReferenceId;
        public string txnReferenceId
        {
            get => _txnReferenceId.TrimIfNotNull();
            set => _txnReferenceId = value.TrimIfNotNull();
        }
    }
    public class ComplaintAPIRequest
    {
        public ComplainReq complainReq { get; set; }

        private string _msgType;
        public string msgType
        {
            get => _msgType.TrimIfNotNull();
            set => _msgType = value.TrimIfNotNull();
        }
        private string _xchangeId;
        public string xchangeId
        {
            get => _xchangeId.TrimIfNotNull();
            set => _xchangeId = value.TrimIfNotNull();
        }

        private string _EncryptionKey;

        public string EncryptionKey
        {
            get => _EncryptionKey.TrimIfNotNull();
            set => _EncryptionKey = value.TrimIfNotNull();
        }
    }
}
