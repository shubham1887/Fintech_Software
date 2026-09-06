using Project.Processing;
using Project.Response.AEPS;
using System.Collections.Generic;

namespace Project.Response.TransactionMaster
{
    public class RechargeReceiptViewModel
    {
        public int UserID { get; set; }

        public int OperatorID { get; set; }

        public int ServiceID { get; set; }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _CustomerNumber;

        public string CustomerNumber
        {
            get => _CustomerNumber.TrimIfNotNull();
            set => _CustomerNumber = value.TrimIfNotNull();
        }
        
       
        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal decimal AmountDecimal { set; get; }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.ToUpperCase();
            set => _ReferenceNo = value.ToUpperCase();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.ToUpperCase();
            set => _ProviderRequestID = value.ToUpperCase();
        }
        private string _BillerID;

        public string BillerID
        {
            get => _BillerID.ToUpperCase();
            set => _BillerID = value.ToUpperCase();
        }
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.ToUpperCase();
            set => _BillDate = value.ToUpperCase();
        }

        private string _ProviderRefNo;

        public string ProviderRefNo
        {
            get => _ProviderRefNo.ToUpperCase();
            set => _ProviderRefNo = value.ToUpperCase();
        }
        
        private decimal _NetCommission;

        public decimal NetCommission
        {
            get => _NetCommission;
            set => _NetCommission = value;
        }

        private string _FirmAddress;

        public string FirmAddress
        {
            get => _FirmAddress.TrimIfNotNull();
            set => _FirmAddress = value.TrimIfNotNull();
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
        
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }
        
        private string _STANNo;

        public string STANNo
        {
            get => _STANNo.TrimIfNotNull();
            set => _STANNo = value.TrimIfNotNull();
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
        
        private string _AccBalance;

        public string AccBalance
        {
            get => _AccBalance.TrimIfNotNull();
            set => _AccBalance = value.TrimIfNotNull();
        }

        private string _AEPSResponseJSON;

        public string AEPSResponseJSON
        {
            get => _AEPSResponseJSON.TrimIfNotNull();
            set => _AEPSResponseJSON = value.TrimIfNotNull();
        }

        private string _OperatorImagePath;

        public string OperatorImagePath
        {
            get => _OperatorImagePath;
            set => _OperatorImagePath = value;
        }

        private string _DMT_BeneficiaryName;

        public string DMT_BeneficiaryName
        {
            get => _DMT_BeneficiaryName.TrimIfNotNull();
            set => _DMT_BeneficiaryName = value.TrimIfNotNull();
        }

        internal int DistributorID;

        internal int SuperDistributorID;

        private List<Ministatement> _ministatements;

        public List<Ministatement> ministatements
        {
            get => _ministatements;
            set => _ministatements = value;
        }

        private string _BBPS_CustomerName;

        public string BBPS_CustomerName
        {
            get => _BBPS_CustomerName;
            set => _BBPS_CustomerName = value;
        }

        public bool? IsBBPS { get; internal set; }

        private string _PickupLocation;

        public string PickupLocation
        {
            get => _PickupLocation;
            set => _PickupLocation = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }
        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
        }
        private decimal _RetailerGST;

        public decimal RetailerGST
        {
            get => _RetailerGST;
            set => _RetailerGST = value;
        }
        private decimal _ChargeAmount;

        public decimal ChargeAmount
        {
            get => _ChargeAmount;
            set => _ChargeAmount = value;
        }

    }
}
