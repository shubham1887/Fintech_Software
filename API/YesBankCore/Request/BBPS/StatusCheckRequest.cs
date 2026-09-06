using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class StatusCheckRequest
    {

        private string _BillerId;

        public string BillerId
        {
            get => _BillerId.TrimIfNotNull();
            set => _BillerId = value.TrimIfNotNull();
        }
        private string _TxnReferenceId;

        public string TxnReferenceId
        {
            get => _TxnReferenceId.TrimIfNotNull();
            set => _TxnReferenceId = value.TrimIfNotNull();
        }
        private string _XchangeId;

        public string XchangeId
        {
            get => _XchangeId.TrimIfNotNull();
            set => _XchangeId = value.TrimIfNotNull();
        }

        
    }
}
