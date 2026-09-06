using Project.Processing;
using System;

namespace Project.Response.UserAccounting
{
    public class OldAmountAdjustmentViewModel
    {

        
        private string _DOC;

        public string DOC
        {
            get => _DOC;
            set => _DOC = value;
        }
        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC;
            set => _UpdatedDOC = value;
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
        
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }
        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }
        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
        }
        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }
        
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

    }
}
