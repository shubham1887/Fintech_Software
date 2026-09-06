using Microsoft.VisualBasic;
using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class TransactionResponse
    {
        private string _DateAndTime;

        public string DateAndTime
        {
            get => _DateAndTime.TrimIfNotNull();
            set => _DateAndTime = value.TrimIfNotNull();
        }
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
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }
        private string _Charges;

        public string Charges
        {
            get => _Charges.TrimIfNotNull();
            set => _Charges = value.TrimIfNotNull();
        }
        private string _GST;

        public string GST
        {
            get => _GST.TrimIfNotNull();
            set => _GST = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        
        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _RefrenceID;

        public string RefrenceID
        {
            get => _RefrenceID.TrimIfNotNull();
            set => _RefrenceID = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private int _Operator;

        public int Operator
        {
            get => _Operator;
            set => _Operator = value;
        }

        private int _VendorID;

        public int VendorID
        {
            get => _VendorID;
            set => _VendorID = value;
        }

        private bool _IsOTP;

        public bool IsOTP
        {
            get => _IsOTP;
            set => _IsOTP = value;
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }
        
        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }
        
        private string _BillerID;

        public string BillerID
        {
            get => _BillerID.TrimIfNotNull();
            set => _BillerID = value.TrimIfNotNull();
        }
        
        private string _ConsumerName;

        public string ConsumerName
        {
            get => _ConsumerName.TrimIfNotNull();
            set => _ConsumerName = value.TrimIfNotNull();
        }

        private string _BeneficiaryName;

        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        } 
        
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        } 
        
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
        
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.TrimIfNotNull();
            set => _BillDate = value.TrimIfNotNull();
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
    }
}
