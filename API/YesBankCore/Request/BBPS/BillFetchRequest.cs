using FluentValidation;
using FluentValidation.Results;
using YesBankCore.Controller;
using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class BillFetchRequest
    {
        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        private string _BillerCode;

        public string BillerCode
        {
            get => _BillerCode.TrimIfNotNull();
            set => _BillerCode = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _BillerId;

        public string BillerId
        {
            get => _BillerId.TrimIfNotNull();
            set => _BillerId = value.TrimIfNotNull();
        }

        private string _param1;

        public string param1
        {
            get => _param1.TrimIfNotNull();
            set => _param1 = value.TrimIfNotNull();
        }

        private string _param2;

        public string param2
        {
            get => _param2.TrimIfNotNull();
            set => _param2 = value.TrimIfNotNull();
        }
        
        private string _param3;

        public string param3
        {
            get => _param3.TrimIfNotNull();
            set => _param3 = value.TrimIfNotNull();
        }

        private string _param4;

        public string param4
        {
            get => _param4.TrimIfNotNull();
            set => _param4 = value.TrimIfNotNull();
        }
        
        private string _param5;

        public string param5
        {
            get => _param5.TrimIfNotNull();
            set => _param5 = value.TrimIfNotNull();
        }

        
        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }
        
        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BillFetchRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BillFetchRequestValidator : AbstractValidator<BillFetchRequest>
    {
        public BillFetchRequestValidator()
        {

        }
    }
}