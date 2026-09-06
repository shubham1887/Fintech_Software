using Project.Processing;
using System;

namespace Project.Response.TransactionMaster
{
    public class StatusCheckResponse
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _CustomerNumber;

        public string CustomerNumber
        {
            get => _CustomerNumber.TrimIfNotNull();
            set => _CustomerNumber = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private Decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
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

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private int transactionMasterID;

        public int TransactionMasterID
        {
            get => transactionMasterID;
            set => transactionMasterID = value;
        }
    }
}