using Project.Processing;
using System;

namespace Project.Response.TransactionMaster
{
    public class PendingResponse
    {
        public int TransactionMasterID { set; get; }

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

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        public decimal Amount { set; get; }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _AdditionalParam1;

        public string AdditionalParam1
        {
            get => _AdditionalParam1.TrimIfNotNull();
            set => _AdditionalParam1 = value.TrimIfNotNull();
        }

        private string _AdditionalParam2;

        public string AdditionalParam2
        {
            get => _AdditionalParam2.TrimIfNotNull();
            set => _AdditionalParam2 = value.TrimIfNotNull();
        }

        private DateTime _OriginalDOC;

        public DateTime OriginalDOC
        {
            get => _OriginalDOC;
            set => _OriginalDOC = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _SenderNo;

        public string SenderNo
        {
            get => _SenderNo.TrimIfNotNull();
            set => _SenderNo = value.TrimIfNotNull();
        }

        private string _Bank;

        public string Bank
        {
            get => _Bank.TrimIfNotNull();
            set => _Bank = value.TrimIfNotNull();
        }

        private string _PaymentType;

        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _BeneficiaryName;

        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }

        private string _SenderName;

        public string SenderName
        {
            get => _SenderName.TrimIfNotNull();
            set => _SenderName = value.TrimIfNotNull();
        }   
    }
}