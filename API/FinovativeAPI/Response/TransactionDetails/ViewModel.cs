using Project.Processing;
using System;

namespace Project.Response.TransactionDetails
{
    public class ViewModel
    {
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
        
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }
        
        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }
        
        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _DOCType;

        public string DOCType
        {
            get => _DOCType.TrimIfNotNull();
            set => _DOCType = value.TrimIfNotNull();
        }

        private string consumerNumber;

        public string ConsumerNumber
        {
            get => consumerNumber.TrimIfNotNull();
            set => consumerNumber = value.TrimIfNotNull();
        }

        private decimal openingBalance;

        public decimal OpeningBalance
        {
            get => openingBalance;
            set => openingBalance = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal discountAmount;

        public decimal DiscountAmount
        {
            get => discountAmount;
            set => discountAmount = value;
        }

        private decimal closingBalance;

        public decimal ClosingBalance
        {
            get => closingBalance;
            set => closingBalance = value;
        }

        private string _AmountCreditDebit;

        public string AmountCreditDebit
        {
            get => _AmountCreditDebit;
            set => _AmountCreditDebit = value;
        }

        private string assignedUserName;

        public string AssignedUserName
        {
            get => assignedUserName.TrimIfNotNull();
            set => assignedUserName = value.TrimIfNotNull();
        }

        private string transactionDescription;

        public string TransactionDescription
        {
            get => transactionDescription.TrimIfNotNull();
            set => transactionDescription = value.TrimIfNotNull();
        }

        private int _ReferenceID;

        public int ReferenceID
        {
            get => _ReferenceID;
            set => _ReferenceID = value;
        }

        private int transactionDetailID;

        public int TransactionDetailID
        {
            get => transactionDetailID;
            set => transactionDetailID = value;
        }

        private int userID;

        public int UserID
        {
            get => userID;
            set => userID = value;
        }

        private int assignedUserID;

        public int AssignedUserID
        {
            get => assignedUserID;
            set => assignedUserID = value;
        }

        private int operatorID;

        public int OperatorID
        {
            get => operatorID;
            set => operatorID = value;
        }

        private int serviceID;

        public int ServiceID
        {
            get => serviceID;
            set => serviceID = value;
        }

        private int walletID;

        public int WalletID
        {
            get => walletID;
            set => walletID = value;
        }

        private decimal transactionAmount;

        public decimal TransactionAmount
        {
            get => transactionAmount;
            set => transactionAmount = value;
        }

        private decimal gST;

        public decimal GST
        {
            get => gST;
            set => gST = value;
        }

        private decimal tDS;

        public decimal TDS
        {
            get => tDS;
            set => tDS = value;
        }

        private string _CommissionCreditDebit;

        public string CommissionCreditDebit
        {
            get => _CommissionCreditDebit;
            set => _CommissionCreditDebit = value;
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }
        
        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }
        
        private decimal _NetCommission;

        public decimal NetCommission
        {
            get => _NetCommission;
            set => _NetCommission = value;
        }

        private string _ProviderRefNo;

        public string ProviderRefNo
        {
            get => _ProviderRefNo.TrimIfNotNull();
            set => _ProviderRefNo = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private DateTime _OriginalDOC;

        public DateTime OriginalDOC
        {
            get => _OriginalDOC;
            set => _OriginalDOC = value;
        }
        private string _RechargeDate;

        public string RechargeDate
        {
            get => _RechargeDate.TrimIfNotNull();
            set => _RechargeDate = value.TrimIfNotNull();
        }

    }
}