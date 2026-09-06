using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.AEPS
{
    public class AEPSViewModelResponse
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

        private string _BankRRN;

        public string BankRRN
        {
            get => _BankRRN.TrimIfNotNull();
            set => _BankRRN = value.TrimIfNotNull();
        }

        private string _AvailableBalance;

        public string AvailableBalance
        {
            get => _AvailableBalance.TrimIfNotNull();
            set => _AvailableBalance = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }


        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }
        
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        
        private string _Stan;

        public string Stan
        {
            get => _Stan.TrimIfNotNull();
            set => _Stan = value.TrimIfNotNull();
        }

        private int _VendorID;

        public int VendorID
        {
            get => _VendorID;
            set => _VendorID = value;
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }
        
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
        
        private string _AgentID;

        public string AgentID
        {
            get => _AgentID.TrimIfNotNull();
            set => _AgentID = value.TrimIfNotNull();
        }
        
        private string _AgentName;

        public string AgentName
        {
            get => _AgentName.TrimIfNotNull();
            set => _AgentName = value.TrimIfNotNull();
        }

        private List<Ministatement> _ministatements;

        public List<Ministatement> ministatements
        {
            get => _ministatements;
            set => _ministatements = value;
        }
        public List<string> ministatementStr { get; set; }
      
    }

    public class Ministatement
    {
        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private string _TxnType;

        public string TxnType
        {
            get => _TxnType.TrimIfNotNull();
            set => _TxnType = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }
    }
}