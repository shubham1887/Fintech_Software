using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class ComplainReq1
    {
        private string _txnReferenceId;
        public string txnReferenceId
        {
            get => _txnReferenceId.TrimIfNotNull();
            set => _txnReferenceId = value.TrimIfNotNull();
        }

        private string _complaintType;
        public string complaintType
        {
            get => _complaintType.TrimIfNotNull();
            set => _complaintType = value.TrimIfNotNull();
        }
    }
    public class ComplaintStatusAPIRequest
    {

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

        public ComplainReq1 complainReq { get; set; }
    }
}
