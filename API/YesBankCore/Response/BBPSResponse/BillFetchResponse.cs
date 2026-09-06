using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class BillFetchResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        private string _BillDueDate;

        public string BillDueDate
        {
            get => _BillDueDate.TrimIfNotNull();
            set => _BillDueDate = value.TrimIfNotNull();
        }
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.TrimIfNotNull();
            set => _BillDate = value.TrimIfNotNull();
        }
        
        private string _BillerID;

        public string BillerID
        {
            get => _BillerID.TrimIfNotNull();
            set => _BillerID = value.TrimIfNotNull();
        }
        
        private string _BillerName;

        public string BillerName
        {
            get => _BillerName.TrimIfNotNull();
            set => _BillerName = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _BillValidateId;

        public string BillValidateId
        {
            get => _BillValidateId.TrimIfNotNull();
            set => _BillValidateId = value.TrimIfNotNull();
        }
    }
}
